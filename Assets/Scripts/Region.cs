using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RegionState { A, B, C }
public enum RegionEffect { Stable, Unstable, Volatile }
public enum Actions { Shift, Flip,  }

[RequireComponent(typeof(RegionOutline))]
public class Region : MonoBehaviour {

    public RegionState State { get { return currentState; }
        set
        {
            currentState = value;
            updateMaterials();
        }
    }
    public IEnumerable<GameObject> Neighbours { get { return neighbours.AsEnumerable(); } }

    [Header("Set-up properties")]
    public RegionState initialState = RegionState.A;
    private Material material;
    [Header("Runtime properties")]
    public RegionState currentState;
    [Header("Editor properties (board design)")]
    public GameObject[] hexTilesToAdd;

    public static RegionEffect StateToEffect(RegionState state, PlayerID player)
    {
        if (state == RegionState.C) return RegionEffect.Volatile;
        else return player == PlayerID.P1 ? (RegionEffect)state : (RegionEffect)(((int)state - 1) * -1);
    }

    [SerializeField]
    private GameObject[] neighbours;
    [SerializeField]
    private List<GameObject> hexTiles = new List<GameObject>();
    [SerializeField]
    private Material[] tileMaterials = new Material[3];
    [SerializeField]
    private Material[] outlineMaterials = new Material[3];

    void Start () {
        init();
	}

    void init()
    {
        refresh();
    }

    public void ShiftState(int offset) {
        State = (RegionState)(((int)State + offset) % 3);
    }

    [ContextMenu("Delete region")]
    public void Delete() {
        if (Application.isPlaying) Destroy(gameObject);
        else DestroyImmediate(gameObject);
    }

    public void OnDestroy()
    {
        GameBoard board = GetComponentInParent<GameBoard>();
        if (board == null) return;
        board.regions.Remove(gameObject);
    }

    /// <summary>
    /// Consolidates the tiles in `hexTilesToAdd` into the region.
    /// </summary>
    [ContextMenu("Consolidate new tiles")]
    public void Consolidate() {
        if (hexTilesToAdd == null || hexTilesToAdd.Length == 0) return;
        IEnumerable<HexMesh> newTiles = hexTilesToAdd.Select(t => t.GetComponent<HexMesh>()).Where(hex => hex != null);
        if (!isContiguous(hexTilesToAdd)) throw new System.Exception("Region tiles must be contiguous.");

        foreach (HexMesh tile in newTiles)
        {   
            HexMesh tileHexMesh = tile.GetComponent<HexMesh>();
            // Release tile from current parent (either region or generator)
            HexGridGenerator mom = tile.transform.parent.GetComponent<HexGridGenerator>();
            Region dad = tile.transform.parent.GetComponent<Region>();
            if (mom != null) mom.ReleaseTile(tileHexMesh.Location.row, tileHexMesh.Location.offset);
            else if (dad != null) dad.ReleaseTile(tile.gameObject);

            if (Application.isPlaying)
                Destroy(tile.GetComponent<LineRenderer>());
            else DestroyImmediate(tile.GetComponent<LineRenderer>());

            tile.transform.SetParent(transform);
            hexTiles.Add(tile.gameObject);
        }
        hexTilesToAdd = null;
        refresh();
    }

    void refreshNeighbours() {
        HashSet<GameObject> neighboursTmp = new HashSet<GameObject>();
        foreach (HexMesh hex in hexTiles.Select(tile => tile.GetComponent<HexMesh>())) {
            if (hex == null) continue; // sanity!
            neighboursTmp.UnionWith(hex.Edges.Where(obj => obj == null ? false : obj.transform.parent != transform));
        }
        neighbours = neighboursTmp.ToArray();
    }

    public List<Vector3> GetBorderVertices(float insideBorder)
    {
        insideBorder *= 0.95f;
        int tileNum = 0;
        GameObject startingTile = null;
        int startingEdge = -1;
        while (startingEdge < 0) {
            if (tileNum >= hexTiles.Count) throw new System.Exception("Could not find a suitable outer tile for the region. Data error.");
            startingTile = hexTiles[tileNum];
            tileNum++;
            startingEdge = findOuterEdge(startingTile.GetComponent<HexMesh>());
        }

        List<Vector3> vertices = new List<Vector3>();
        GameObject currTile = startingTile;
        GameObject nextTile = null;
        int currEdge = startingEdge;
        int nextEdge = -1;
        bool concaveVertex = false;
        HexMesh hex = currTile.GetComponent<HexMesh>();
        Vector3 vertex = hex.transform.position + hex.OuterVertices[currEdge];
        vertices.Add(shiftPointInsideRegion(vertex, hex, currEdge, concaveVertex, insideBorder));
        // Advance around hex edges in a clockwise direction
        while (currTile != startingTile || currEdge != startingEdge || vertices.Count == 1)
        {
            nextTile = currTile;
            nextEdge = (currEdge + 1) % 6;
            if (hexTiles.Contains(hex.Edges[nextEdge]))
            {
                concaveVertex = true;
                nextTile = hex.Edges[nextEdge];
                nextEdge = (nextEdge + 4) % 6;
            }
            else concaveVertex = false;
            vertex = hex.transform.position + hex.OuterVertices[(currEdge + 1) % 6];
            vertices.Add(shiftPointInsideRegion(vertex, hex, currEdge, concaveVertex, insideBorder));
            // Advance to the next edge on the hex
            currEdge = nextEdge;
            currTile = nextTile;
            hex = currTile.GetComponent<HexMesh>();
        }
        return vertices;
    }

    private Vector3 shiftPointInsideRegion(Vector3 vertex, HexMesh hex, int currEdge, bool concaveVertex, float amt)
    {
        Vector3 shiftDirection = hex.transform.position + (concaveVertex ? hex.OuterVertices[(currEdge + 2) % 6] : Vector3.zero);
        return Vector3.MoveTowards(vertex, shiftDirection, 0.5f * amt / Mathf.Cos(Mathf.Deg2Rad * 30));
    }

    public void ReleaseTile(GameObject tile)
    {
        hexTiles.Remove(tile);
        refresh();
    }

    [ContextMenu("Refresh")]
    private void refresh()
    {
        currentState = initialState;
        refreshColliders();
        GetComponent<RegionOutline>().Refresh();
        updateMaterials();
    }

    private bool isContiguous(GameObject[] tiles) {
        if (tiles.Length == 1) return true;
        if (tiles.Length == 2) return tiles[0].GetComponent<HexMesh>().Edges.Contains(tiles[1]);
        int n = 0;

        // BFS to check for a path to all tiles
        Queue<GameObject> q = new Queue<GameObject>();
        HashSet<GameObject> visited = new HashSet<GameObject>();
        q.Enqueue(tiles[0]);
        visited.Add(tiles[0]);
        while (q.Count > 0 && n <= tiles.Length)
        {
            GameObject node = q.Dequeue();
            foreach (GameObject neighbour in node.GetComponent<HexMesh>().Edges.Where(edge => tiles.Contains(edge) && edge != null))
            {
                if (!visited.Contains(neighbour))
                {
                    q.Enqueue(neighbour);
                    visited.Add(neighbour);
                }
            }
            n++;
        }
        return visited.Count == tiles.Length;
    }

    private void refreshColliders()
    {
        Collider[] colliders = GetComponents<Collider>();
        if (colliders != null)
            foreach (Collider c in colliders)
                if (Application.isPlaying) Destroy(c);
                else DestroyImmediate(c);

        foreach (GameObject tile in hexTiles)
        {
            Mesh oldMesh = tile.GetComponent<MeshFilter>().sharedMesh;
            MeshCollider mc = gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = new Mesh();
            mc.sharedMesh.vertices = oldMesh.vertices.Select(v => tile.transform.TransformPoint(v) - transform.position).ToArray();
            mc.sharedMesh.triangles = oldMesh.triangles;
            mc.convex = true;
            mc.isTrigger = true;
        }
    }

    private int findOuterEdge(HexMesh hex) {
        for (int i = 0; i < 6; i++) {
            if (hex.Edges[i] == null) continue;
            if (!hexTiles.Contains(hex.Edges[i]))
                return i;
        }
        return -1;
    }

    [ContextMenu("Update materials")]
    private void updateMaterials() {
        if (tileMaterials != null && tileMaterials[(int)State] != null)
            foreach (GameObject tile in hexTiles)
                tile.transform.GetComponent<MeshRenderer>().material = tileMaterials[(int)State];
        RegionOutline outline = GetComponent<RegionOutline>();
        if (outline != null && outlineMaterials != null && outlineMaterials[(int)State] != null)
        {
            outline.Material = outlineMaterials[(int)State];
        }
    }

    public void SetTileMaterial(RegionState state, Material tileMaterial) {
        tileMaterials[(int)state] = tileMaterial;
        if (state == currentState) updateMaterials();
    }

    public void SetOutlineMaterial(RegionState state, Material outlineMaterial)
    {
        outlineMaterials[(int)state] = outlineMaterial;
        if (state == currentState) updateMaterials();
    }

    public Material[] TileMaterials
    {
        set
        {
            tileMaterials = value;
            updateMaterials();
        }
        get { return (Material[])tileMaterials.Clone(); }
    }

    public Material[] OutlineMaterials
    {
        set
        {
            outlineMaterials = value;
            updateMaterials();
        }
        get { return (Material[])outlineMaterials.Clone(); }
    }
}

public class NoMeshStoredException : System.Exception
{
    public NoMeshStoredException() { }
    public NoMeshStoredException(string message) : base(message) { }
    public NoMeshStoredException(string message, System.Exception inner) : base(message, inner) { }
}
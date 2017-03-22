using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RegionOutline))]
public class RegionBuilder : Region {
    [Header("Editor properties (board design)")]
    public GameObject[] hexTilesToAdd;

    private GameObject goalGameObject;

    new void Start ()
    {
        base.Start();
    }

    void init()
    {
        rebuild();
    }

    /// <summary>
    /// Helper method to use when state changes occur during the board building process.
    /// </summary>
    [ContextMenu("Rebuild")]
    void rebuild()
    {
        currentState = initialState;
        findNeighbours();
        RegionOutline outline = GetComponent<RegionOutline>();
        outline.Vertices = getBorderVertices(outline.baseLineSize).ToArray();

		addPlayerGoal ();
	
        refresh(); // base class method
    }

	void addPlayerGoal ()
	{
		if (!isGoal ())
			return;

		if (getPlayerGoal () == null) {
			Debug.Log ("Player goal object is not set!");
			return;
		}


		goalGameObject = Instantiate (getPlayerGoal (), this[0].transform.position, Quaternion.identity);
	    goalGameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = tileMaterials[(int) State];

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
            if (tileHexMesh == null) continue;
            // Release tile from current parent (either region or generator)
            RegionBuilder mom = tile.transform.parent.GetComponent<RegionBuilder>();
            if (mom != null) mom.ReleaseTile(tile.gameObject);

            if (Application.isPlaying)
                Destroy(tile.GetComponent<LineRenderer>());
            else DestroyImmediate(tile.GetComponent<LineRenderer>());

            tile.transform.SetParent(transform);
            hexTiles.Add(tile.gameObject);
        }
        hexTilesToAdd = null;
		rebuild();
    }

    [ContextMenu("Delete region")]
    public void Delete()
    {
        if (Application.isPlaying) Destroy(gameObject);
        else DestroyImmediate(gameObject);
    }

    public void OnDestroy()
    {
        GameBoard board = GetComponentInParent<GameBoard>();
        if (board == null) return;
        board.regions.Remove(gameObject);
    }

    private void refreshNeighbours() {
        HashSet<GameObject> neighboursTmp = new HashSet<GameObject>();
        foreach (HexMesh hex in hexTiles.Select(tile => tile.GetComponent<HexMesh>())) {
            if (hex == null) continue; // sanity!
            neighboursTmp.UnionWith(hex.Edges.Where(obj => obj == null ? false : obj.transform.parent != transform));
        }
        neighbours = neighboursTmp.ToArray();
    }

    private List<Vector3> getBorderVertices(float insideBorder)
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
        Vector3 vertex = hex.transform.position - transform.position + hex.OuterVertices[currEdge];
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
            vertex = hex.transform.position - transform.position + hex.OuterVertices[(currEdge + 1) % 6];
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
        Vector3 shiftDirection = hex.transform.position - transform.position + (concaveVertex ? hex.OuterVertices[(currEdge + 2) % 6] : Vector3.zero);
        return Vector3.MoveTowards(vertex, shiftDirection, 0.5f * amt / Mathf.Cos(Mathf.Deg2Rad * 30));
    }

    public void ReleaseTile(GameObject tile)
    {
        if (!hexTiles.Remove(tile)) return;
        tile.transform.SetParent(transform.parent);
        if (hexTiles.Count == 0) Delete();
        else rebuild();
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

    private void findNeighbours() {
        neighbours = hexTiles
            .Select(t => t.GetComponent<HexMesh>())
            .Where(hex => hex != null)
            .SelectMany(hex => 
                hex.Edges
                    .Where(edge => edge != null)
                    .Select(edge => edge.transform.parent))
            .Distinct()
            .Where(t => t != transform && t.GetComponent<Region>() != null)
            .Select(t => t.gameObject)
            .ToArray();
    }

    private int findOuterEdge(HexMesh hex) {
        for (int i = 0; i < 6; i++) {
            if (hex.Edges[i] == null) continue;
            if (!hexTiles.Contains(hex.Edges[i]))
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Change the tile surface material that corresponds to a given state.  Do not use this method to change the current state.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="tileMaterial"></param>
    public void SetTileMaterial(RegionState state, Material tileMaterial) {
        tileMaterials[(int)state] = tileMaterial;
        if (state == currentState) updateMaterials();
    }

    /// <summary>
    /// Change the tile outline material that corresponds to a given state.  Do not use this method to change the current state.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="outlineMaterial"></param>
    public void SetOutlineMaterial(RegionState state, Material outlineMaterial)
    {
        outlineMaterials[(int)state] = outlineMaterial;
        if (state == currentState) updateMaterials();
    }

    /// <summary>
    /// The tile surface materials for this region.
    /// </summary>
    public Material[] TileMaterials
    {
        set
        {
            tileMaterials = value;
            updateMaterials();
        }
        get { return (Material[])tileMaterials.Clone(); }
    }

    /// <summary>
    /// The tile outline materials for this region.
    /// </summary>
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
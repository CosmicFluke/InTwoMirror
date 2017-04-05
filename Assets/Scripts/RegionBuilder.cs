using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RegionOutline))]
public class RegionBuilder : Region {
    [Header("Editor properties (board design)")]
    public GameObject[] hexTilesToAdd;

    new void Start ()
    {
        init();
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
        refreshColliders();
        GetComponent<RegionOutline>().Refresh();
        refresh(); // base class method
    }

    /// <summary>
    /// Consolidates the tiles in `hexTilesToAdd` into the region.
    /// </summary>
    [ContextMenu("Consolidate new tiles")]
    public void Consolidate() {
        if (hexTilesToAdd == null || hexTilesToAdd.Length == 0) return;
        IEnumerable<HexMesh> newTiles = hexTilesToAdd.Select(t => t.GetComponent<HexMesh>()).Where(hex => hex != null);
        if (!isContiguous(hexTiles.Concat(hexTilesToAdd).ToArray())) throw new System.Exception("Region tiles must be contiguous.");

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
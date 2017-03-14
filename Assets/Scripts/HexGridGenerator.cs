using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGridGenerator : MonoBehaviour {

    public GameObject hexPrefab;
    public int width = 4;
    public int length = 5;
    public BoardShape shape = BoardShape.Diamond;

    public bool IsGenerated { get { return isGenerated; } }
    public GameObject this[HexGridCoordinates i] {
        get {
            if (!isGenerated) throw new Exception("Tiles not generated yet.");
            GameObject tile = tiles[i];
            if (tile == null) Debug.Log("Indexer returning null tile");
            return tile;
        }
        set {
            if (tiles.ContainsKey(i) && tiles[i] != null) throw new Exception("Grid already contains tile at this location.");
            tiles[i] = value;
        }
    }

    public bool ContainsTileAt(HexGridCoordinates loc) {
        return tiles.ContainsKey(loc) && this[loc] != null;
    }

    [SerializeField]
    private Dictionary<HexGridCoordinates, GameObject> tiles;
    [SerializeField]
    private bool isGenerated = false;

    private Material origMat;
    private Dictionary<BoardShape, Func<int, int>> widthFunctions;

    // Use this for initialization
    void Awake() {
        Init();
    }

    void Init() {
        Debug.Log("Initializing tiles!");
        tiles = new Dictionary<HexGridCoordinates, GameObject>();
        setWidthFunctions();
    }

    void setWidthFunctions() {
        widthFunctions = new Dictionary<BoardShape, Func<int, int>> {
            { BoardShape.Rectangle, (row) => row % 2 == 0 ? width - 1 : width },
            { BoardShape.Diamond, (row) => width - Mathf.Abs((length / 2) - row) }
        };
    }

    void Start(){
        if (tiles == null) Init();
    }

    [ContextMenu("Generate tiles")]
    public void Generate() {
        Debug.Log(string.Format("Generating hex grid: width={0}, length={1}, shape={2}", width, length, shape));
        if (tiles == null) Init();
        if (widthFunctions == null) setWidthFunctions();
        if (hexPrefab == null) {
            Debug.Log("Attempted to generate tiles without hexagon prefab.");
            return;
        }
        HexMesh hexMesh = hexPrefab.GetComponent<HexMesh>();
        if (hexMesh == null) {
            Debug.Log("Could not generate hex tiles: hexPrefab does not have a HexMesh component.");
            return;
        }   
        Vector3 parentPos = gameObject.transform.position;
        foreach (int z in Enumerable.Range(0, length))
        {
            int rowWidth = widthFunctions[shape](z);
            if (rowWidth % 2 == 1)
                makeTile(new HexGridCoordinates(z, 0, false), rowWidth);
            foreach (int x in Enumerable.Range(1, rowWidth / 2))
            {
                makeTile(new HexGridCoordinates(z, x, rowWidth % 2 == 0), rowWidth);
                makeTile(new HexGridCoordinates(z, -x, rowWidth % 2 == 0), rowWidth);
            }
        }
        isGenerated = true;
        //refreshGraph();
    }

    /// <summary>
    /// Generates and stores a hex tile for the given position
    /// </summary>
    /// <param name="loc">Tile location, with a row number and centre-offset.</param>
    /// <param name="rowWidth">The width of the row this tile will be placed in.</param>
    private void makeTile(HexGridCoordinates loc, int rowWidth) {
        if (loc.offsetType != HexGridCoordinates.OffsetType.Centre) throw new Exception("New tiles must be specified using centre-offset.");
        float outerRadius = hexPrefab.GetComponent<HexMesh>().radius;
        float innerRadius = outerRadius * HexMesh.radiusRatio;
        Vector3 parentPos = gameObject.transform.position;
        float horizontalDistance = parentPos.x + loc.offset * 2 * innerRadius - innerRadius * Mathf.Abs(rowWidth % 2 - 1) * (loc.offset < 0 ? -1 : 1);
        Vector3 pos = new Vector3(horizontalDistance, parentPos.y, parentPos.z + loc.row * 1.5f * outerRadius);
        HexMesh tile = Instantiate(hexPrefab, pos, Quaternion.identity).GetComponent<HexMesh>();
        tile.transform.parent = gameObject.transform;
        tile.Location = loc;
        tile.spawnedBy = gameObject;
        tile.SelfPrefab = hexPrefab;
        tile.gameObject.name = "Hex tile, " + tile.Location.ToString();
        tiles[tile.Location] = tile.gameObject;
        tile.DrawOutline();
        LinkToNeighbours(tile);
    }

    /// <summary>
    /// Forms a graph using the HexMesh components as nodes, with edges to their neighbours
    /// </summary>
    private void refreshGraph() {
        foreach (KeyValuePair<HexGridCoordinates, GameObject> pair in tiles) {
            GameObject tile = pair.Value;
            if (tile == null) continue;
            HexMesh hex = tile.GetComponent<HexMesh>();
            if (hex == null) { Debug.LogError("Generator contains object that is not a hex tile"); continue; }
            if (hex.Location != pair.Key) { Debug.LogError("Location key & tile location property don't match"); }
            HexGridCoordinates loc = pair.Key;
            LinkToNeighbours(hex);
        }
    }

    public void LinkToNeighbours(HexMesh hex)
    {
        GameObject[] edges = hex.Edges;
        for (int edge = 0; edge < 6; edge++)
        {
            if (edges[edge] != null) continue;
            HexGridCoordinates neighbourLoc = EdgeToLocation(hex.Location, edge);
            if (!tiles.TryGetValue(neighbourLoc, out edges[edge])) continue;
            if (edges[edge] != null)
                edges[edge].GetComponent<HexMesh>().Edges[(edge + 3) % 6] = hex.gameObject;
        }
    }

    [ContextMenu("Destroy tiles")]
    public void DestroyAll()
    {
        if (tiles == null) return;
        Debug.Log("Destroying tiles");
        foreach (KeyValuePair<HexGridCoordinates, GameObject> tile in tiles) {
            if (tile.Value != null)
            {
                if (Application.isPlaying) Destroy(tile.Value);
                else DestroyImmediate(tile.Value);
            }
            tiles.Remove(tile.Key);
        }
        tiles = null;
        isGenerated = false;
    }

    public void ReleaseTile(HexGridCoordinates loc) {
        if (tiles.ContainsKey(loc)) tiles.Remove(loc);
        else Debug.Log("Trying to release tile that doesn't exist");
    }

    public static HexGridCoordinates EdgeToLocation(HexGridCoordinates loc, int edge) {
        if (edge < 0 || edge > 5) throw new Exception("Edge number must be 0-5");
        int row, offset;
        switch (edge)
        {
            case 0:
                row = loc.row + 1;
                offset = loc.offset + (((loc.evenRowSize && loc.offset < 0) || (!loc.evenRowSize && loc.offset >= 0)) ? 1 : 0);
                break;
            case 1:
                row = loc.row;
                offset = loc.offset + ((loc.evenRowSize && loc.offset == -1) ? 2 : 1);
                break;
            case 2:
                row = loc.row - 1;
                offset = loc.offset + (((loc.evenRowSize && loc.offset < 0) || (!loc.evenRowSize && loc.offset >= 0)) ? 1 : 0);
                break;
            case 3:
                row = loc.row - 1;
                offset = loc.offset - (((loc.evenRowSize && loc.offset > 0) || (!loc.evenRowSize && loc.offset <= 0)) ? 1 : 0);
                break;
            case 4:
                row = loc.row;
                offset = loc.offset - ((loc.evenRowSize && loc.offset == 1) ? 2 : 1);
                break;
            case 5:
                row = loc.row + 1;
                offset = loc.offset - (((loc.evenRowSize && loc.offset > 0) || (!loc.evenRowSize && loc.offset <= 0)) ? 1 : 0);
                break;
            default:
                row = loc.row;
                offset = loc.offset;
                break;
        }
        return new HexGridCoordinates(row, offset, row == loc.row ? loc.evenRowSize : !loc.evenRowSize);
    }

    //private void OnDrawGizmos()
    //{
    //    if (tiles == null) return;
    //    foreach (GameObject tile in tiles)
    //    {
    //        if (tile == null) continue;
    //        Gizmos.color = Color.blue;
    //        Gizmos.DrawSphere(tile.transform.position + Vector3.up, 0.5f);
    //    }
    //}
}

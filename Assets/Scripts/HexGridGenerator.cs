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
    public GameObject this[int i, int j] { get {
            if (!isGenerated) throw new Exception("Tiles not generated yet.");
            GameObject tile = tiles[i, j];
            if (tile == null) Debug.Log("Indexer returning null tile");
            return tile;
        }}

    [SerializeField]
    private GameObject[,] tiles;
    [SerializeField]
    private bool isGenerated = false;

    private Material origMat;
    private Dictionary<BoardShape, Func<int, int>> widthFunctions;

    // Use this for initialization
    void Awake() {
        Init();
    }

    void Init() {
        tiles = new GameObject[length, width + 1];
        if (hexPrefab == null) hexPrefab = GameObject.Find("Hexagon");
        setWidthFunctions();
    }

    void setWidthFunctions() {
        widthFunctions = new Dictionary<BoardShape, Func<int, int>> {
            { BoardShape.Rectangle, (row) => row % 2 == 0 ? width - 1 : width },
            { BoardShape.Diamond, (row) => width - Mathf.Abs((length / 2) - row) }
        };
    }

    void Start(){
        if (tiles == null || tiles.Length == 0) Init();
    }

    [ContextMenu("Generate tiles")]
    public void Generate() {
        Debug.Log(string.Format("Generating hex grid: width={0}, length={1}, shape={2}", width, length, shape));
        if (tiles == null || widthFunctions == null) Init();
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
            Debug.Log("Building row: " + z + ", rowWidth=" + rowWidth);
            if (rowWidth % 2 == 1)
                makeTile(new HexTileLocation(z, 0, false), rowWidth);
            foreach (int x in Enumerable.Range(1, rowWidth / 2))
            {
                makeTile(new HexTileLocation(z, x, rowWidth % 2 == 0), rowWidth);
                makeTile(new HexTileLocation(z, -x, rowWidth % 2 == 0), rowWidth);
            }
        }
        isGenerated = true;
        makeGraph();
    }

    /// <summary>
    /// Generates and stores a hex tile for the given position
    /// </summary>
    /// <param name="loc">Tile location, with a row number and centre-offset.</param>
    /// <param name="rowWidth">The width of the row this tile will be placed in.</param>
    private void makeTile(HexTileLocation loc, int rowWidth) {
        if (!loc.evenRowSize)
            Debug.Log("Making hex tile at " + loc.ToString());
        if (loc.offsetType != HexTileLocation.OffsetType.Centre) throw new Exception("New tiles must be specified using centre-offset.");
        float outerRadius = hexPrefab.GetComponent<HexMesh>().radius;
        float innerRadius = outerRadius * HexMesh.radiusRatio;
        Vector3 parentPos = gameObject.transform.position;
        float horizontalDistance = parentPos.x + loc.offset * 2 * innerRadius - innerRadius * Mathf.Abs(rowWidth % 2 - 1) * (loc.offset < 0 ? -1 : 1);
        Vector3 pos = new Vector3(horizontalDistance, parentPos.y, parentPos.z + loc.row * 1.5f * outerRadius);
        int indexInRow = OffsetToIndexInRow(loc.offset);
        HexMesh tile = Instantiate(hexPrefab, pos, Quaternion.identity).GetComponent<HexMesh>();
        MeshCollider c = tile.GetComponent<MeshCollider>();
        c.sharedMesh = tile.GetComponent<MeshFilter>().sharedMesh;
        tile.transform.parent = gameObject.transform;
        tile.Location = loc;
        tiles[loc.row, indexInRow] = tile.gameObject;
        tile.DrawOutline();
    }

    /// <summary>
    /// Forms a graph using the HexMesh components as nodes, with edges to their neighbours
    /// </summary>
    private void makeGraph() {
        foreach (GameObject tile in tiles) {
            if (tile == null) continue;
            HexMesh hex = tile.GetComponent<HexMesh>();
            HexTileLocation loc = hex.Location;
            GameObject[] edges = hex.Edges;
            for (int edge = 0; edge < 6; edge++)
            {
                if (edges[edge] != null) continue;
                HexTileLocation neighbourLoc = EdgeToLocation(hex.Location, edge);
                int row = neighbourLoc.row, offset = neighbourLoc.offset;
                if (row >= length || row < 0 || Mathf.Abs(offset) > widthFunctions[shape](row) / 2) continue;
                edges[edge] = tiles[row, OffsetToIndexInRow(offset)];
                if (edges[edge] == null) continue;
                edges[edge].GetComponent<HexMesh>().Edges[(edge + 3) % 6] = hex.gameObject;
            }
        }
    }

    public int OffsetToIndexInRow(int offset) {
        return (width + 1 + offset) % (width + 1);
    }

    [ContextMenu("Destroy tiles")]
    public void DestroyAll()
    {
        if (tiles == null) return;
        Debug.Log("Destroying tiles");
        foreach (GameObject tile in tiles) {
            if (tile != null)
            {
                if (Application.isPlaying) Destroy(tile);
                else DestroyImmediate(tile);
            }
        }
        tiles = null;
        isGenerated = false;
    }

    public GameObject ReleaseTile(int row, int offset)
    {
        GameObject tile;
        try
        {
            tile = tiles[row, OffsetToIndexInRow(offset)];
        }
        catch (NullReferenceException e) {
            Debug.Log(string.Format("Null ref at Release Tile.\nRow: {0}  Offset: {1}", row, offset));
            throw e;
        }
        if (tile == null) Debug.Log("Tile not found.");
        tiles[row, OffsetToIndexInRow(offset)] = null;
        return tile;
    }

    public void DeleteTile(int row, int offset)
    {
        Destroy(ReleaseTile(row, offset));
    }

    public static HexTileLocation EdgeToLocation(HexTileLocation loc, int edge) {
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
        return new HexTileLocation(row, offset, row == loc.row ? loc.evenRowSize : !loc.evenRowSize);
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

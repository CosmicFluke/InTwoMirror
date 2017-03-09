using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof(BoardConfiguration))]
public class HexGridGenerator : MonoBehaviour {

    public GameObject hexPrefab;
    public int width = 4;
    public int length = 5;
    public BoardShape shape = BoardShape.Diamond;

    public bool Generated { get { return isGenerated; } }

    public GameObject this[int i, int j] { get { return tiles[i, j]; } }

    private GameObject[,] tiles;
    private Material origMat;

    private bool isGenerated = false;

    private Dictionary<BoardShape, Func<int, int>> widthFunctions;

    // Use this for initialization
    void Awake() {
        tiles = new GameObject[length, width + 1];
        setWidthFunctions();
    }

    void setWidthFunctions() {
        widthFunctions = new Dictionary<BoardShape, Func<int, int>> {
            { BoardShape.Rectangle, (row) => row % 2 == 0 ? width - 1 : width },
            { BoardShape.Diamond, (row) => width - Mathf.Abs((length / 2) - row) }
        };
    }

    void Start(){
        if (hexPrefab == null) hexPrefab = GameObject.Find("Hexagon");
    }

    [ContextMenu("Generate tiles")]
    public void Generate() {
        if (widthFunctions == null)
            setWidthFunctions();
        if (tiles == null) tiles = new GameObject[length, width + 1];
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
                makeTile(new HexTileLocation(z, 0, rowWidth % 2 == 0), rowWidth);
            foreach (int x in Enumerable.Range(Mathf.Abs(width % 2 - 1), rowWidth / 2))
            {
                makeTile(new HexTileLocation(z, x, width % 2 == 0), rowWidth);
                makeTile(new HexTileLocation(z, -x, width % 2 == 0), rowWidth);
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
        if (loc.offsetType != HexTileLocation.OffsetType.Centre) throw new Exception("New tiles must be specified using centre-offset.");
        float outerRadius = hexPrefab.GetComponent<HexMesh>().radius;
        float innerRadius = outerRadius * HexMesh.radiusRatio;
        Vector3 parentPos = gameObject.transform.position;
        float horizontalDistance = parentPos.x + loc.offset * 2 * innerRadius - innerRadius * Mathf.Abs(rowWidth % 2 - 1) * (loc.offset < 0 ? -1 : 1);
        Vector3 pos = new Vector3(horizontalDistance, parentPos.y, parentPos.z + loc.row * 1.5f * outerRadius);
        int indexInRow = OffsetToIndexInRow(loc.offset);
        tiles[loc.row, indexInRow] = Instantiate(hexPrefab, pos, Quaternion.identity);
        tiles[loc.row, indexInRow].transform.parent = gameObject.transform;
        tiles[loc.row, indexInRow].GetComponent<HexMesh>().Location = loc;
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
                int row, offset;
                switch (edge) {
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
                        continue;
                }
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
	
	// Update is called once per frame
	void Update () {
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

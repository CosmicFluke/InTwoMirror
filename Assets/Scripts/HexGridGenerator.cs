using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGridGenerator : MonoBehaviour {

    [Serializable]
    public class TileDictionary : SerializableDictionary<HexGridCoordinates, GameObject> { }

    public GameObject hexPrefab;
    public GameObject hexWallPrefab;
    public int width = 4;
    public int length = 5;
    public BoardShape shape = BoardShape.Diamond;

    [SerializeField]
    private TileDictionary tiles = new TileDictionary();
    [SerializeField]
    private bool isGenerated = false;

    private Material origMat;
    private Dictionary<BoardShape, Func<int, int>> widthFunctions;

    public bool IsGenerated { get { return isGenerated; } }
    public GameObject this[HexGridCoordinates key]
    {
        get
        {
            if (!isGenerated) throw new Exception("Tiles not generated yet.");
            // if (!tiles.ContainsKey(key)) return null;
            GameObject tile = tiles[key];
            if (tile == null) Debug.Log("Indexer returning null tile");
            return tile;
        }
        set
        {
            if (tiles.ContainsKey(key) && tiles[key] != null) throw new Exception("Grid already contains tile at this location.");
            tiles[key] = value;
        }
    }

    public IEnumerable<HexGridCoordinates> TileLocations { get { return tiles.Keys; } }

    public IEnumerator GetEnumerator()
    {
        if (tiles == null) Init();
        return tiles.Keys.GetEnumerator();
    }

    // Use this for initialization
    void Awake() {
        Init();
    }

    void Init() {
        Debug.Log("Initializing tiles!");
        if (tiles == null) tiles = new TileDictionary();
        setWidthFunctions();
    }

    void setWidthFunctions() {
        widthFunctions = new Dictionary<BoardShape, Func<int, int>> {
            { BoardShape.Rectangle, (row) => row % 2 == 0 ? width - 1 : width },
            { BoardShape.Diamond, (row) => width - Mathf.Abs((length / 2) - row) }
        };
    }

    void Start(){
        Init();
    }

    [ContextMenu("Generate tiles")]
    public void Generate() {
        Debug.Log(string.Format("Generating hex grid: width={0}, length={1}, shape={2}", width, length, shape));
        Init();
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
                makeTile(new HexGridCoordinates(z, 0, false));
            foreach (int x in Enumerable.Range(1, rowWidth / 2))
            {
                makeTile(new HexGridCoordinates(z, x, rowWidth % 2 == 0));
                makeTile(new HexGridCoordinates(z, -x, rowWidth % 2 == 0));
            }
        }
        isGenerated = true;
        //refreshGraph();
    }

    /// <summary>
    /// Generates and stores a hex tile for the given position
    /// </summary>
    /// <param name="location">Tile location, with a row number and centre-offset.</param>
    /// <param name="prefab">The object to instantiate.  Must be a hexagon with face perpendicular to Y axis.</param>
    private HexMesh makeTile(HexGridCoordinates location, GameObject prefab, bool linkToNeighbours) {
        if (tiles.ContainsKey(location) && tiles[location] != null) throw new Exception("Attempting to make tile where one already exists.");
        if (location.offsetType != HexGridCoordinates.OffsetType.Centre) throw new Exception("New tiles must be specified using centre-offset.");
        float outerRadius = hexPrefab.GetComponent<HexMesh>().radius;
        Vector3 parentPos = gameObject.transform.position;
        Vector3 pos = parentPos + GetPositionOffset(location, outerRadius);
        HexMesh tile = Instantiate(prefab, pos, Quaternion.identity).GetComponent<HexMesh>();
        tile.transform.parent = gameObject.transform;
        tile.Location = location;
        tile.spawnedBy = gameObject;
        tile.SelfPrefab = hexPrefab;
        tile.gameObject.name = "Hex tile, " + tile.Location.ToString();
        tiles[tile.Location] = tile.gameObject;
        tile.DrawOutline();
        if (linkToNeighbours)
            LinkToNeighbours(tile);
        return tile;
    }

    public HexMesh makeTile(HexGridCoordinates location)
    {
        return makeTile(location, hexPrefab, true);
    }

    public static Vector3 GetPositionOffset(HexGridCoordinates location, float outerRadius) {
        float innerRadius = outerRadius * HexMesh.radiusRatio;
        float horizontalDistance = location.offset * 2 * innerRadius - innerRadius * Convert.ToInt32(location.evenRowSize) * (location.offset < 0 ? -1 : 1);
        return new Vector3(horizontalDistance, 0, location.row * 1.5f * outerRadius);
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
            if (!hex.isWall) LinkToNeighbours(hex);
        }
    }

    public void LinkToNeighbours(HexMesh hex)
    {
        GameObject[] edges = hex.Edges;
        for (int edge = 0; edge < 6; edge++)
        {
            if (edges[edge] != null) continue;
            HexGridCoordinates neighbourLoc = EdgeToLocation(hex.Location, edge);
            GameObject neighbour;
            if (!tiles.TryGetValue(neighbourLoc, out neighbour)) continue;
            if (neighbour != null && !neighbour.GetComponent<HexMesh>().isWall)
            {
                edges[edge] = neighbour;
                edges[edge].GetComponent<HexMesh>().Edges[(edge + 3) % 6] = hex.gameObject;
            }
        }
    }

    [ContextMenu("Destroy walls")]
    public void DestroyWalls()
    {
        foreach (HexGridCoordinates loc in tiles.Keys.ToArray())
        {
            if (tiles[loc].GetComponent<HexMesh>().isWall)
            {
                if (Application.isEditor) DestroyImmediate(tiles[loc]);
                else Destroy(tiles[loc]);
                if (tiles.ContainsKey(loc)) tiles.Remove(loc);
            }
        }
    }

    public void GenerateWalls()
    {
        if (hexWallPrefab == null) return;
        Dictionary<HexGridCoordinates, HexMesh> walls = new Dictionary<HexGridCoordinates, HexMesh>();
        foreach (HexGridCoordinates location in tiles.Keys.ToArray())
        {
            if (!tiles.ContainsKey(location)) continue;
            GameObject tile = tiles[location];
            HexMesh hex;
            if (tile == null) tiles.Remove(location);
            if (tile == null || (hex = tile.GetComponent<HexMesh>()).isWall) continue;
            for (int i = 0; i < 6; i++)
            {
                if (hex.Edges[i] != null) continue;
                HexGridCoordinates loc = EdgeToLocation(hex.Location, i);
                HexMesh borderHex;
                if (!tiles.ContainsKey(loc) && !walls.ContainsKey(loc))
                {
                    borderHex = makeTile(loc, hexWallPrefab, false);
                    walls[loc] = borderHex;
                    borderHex.isWall = true;
                    borderHex.transform.name = "Border Tile " + loc.ToString();
                }
                else if (tiles[loc] != null)
                    continue;
            }
        }
        foreach (KeyValuePair<HexGridCoordinates, HexMesh> entry in walls)
            if (tiles.ContainsKey(entry.Key))
                Debug.Log("Duplicate keys: " + entry.Key + " / " + entry.Value.name);
            else tiles[entry.Key] = entry.Value.gameObject;
    }

    public bool ContainsTileAt(HexGridCoordinates loc)
    {
        return tiles.ContainsKey(loc) && this[loc] != null;
    }

    [ContextMenu("Destroy tiles")]
    public void DestroyAll()
    {
        if (tiles == null) return;
        Debug.Log("Destroying tiles");
        foreach (HexGridCoordinates tileLoc in tiles.Keys.ToArray()) {
            if (tiles[tileLoc] != null)
            {
                if (Application.isPlaying) Destroy(tiles[tileLoc]);
                else DestroyImmediate(tiles[tileLoc]);
            }
            tiles.Remove(tileLoc);
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

    [ContextMenu("Patch!")]
    public void recoverLostTileGrid()
    {
        GameBoard board = transform.parent.GetComponent<GameBoard>();
        if (board == null) throw new Exception("No board to recover tiles from.");
        Region r = board.regions.Where(robj => robj.GetComponent<Region>() != null).First().GetComponent<Region>();
        if (r == null) throw new Exception("No regions in board for tile recovery.");
        HexMesh startingTile = r[0].GetComponent<HexMesh>();
        HexMesh hex;
        TileDictionary visited = new TileDictionary();
        Queue<HexMesh> q = new Queue<HexMesh>();
        q.Enqueue(startingTile);
        while (q.Count > 0)
        {
            hex = q.Dequeue();
            visited[hex.Location] = hex.gameObject;
            IEnumerable<HexMesh> neighbours = hex.Edges
                .Where(n => n != null)
                .Select(n => n.GetComponent<HexMesh>())
                .Where(m => !visited.ContainsKey(m.Location));
            foreach (HexMesh neighbour in neighbours)
            {
                q.Enqueue(neighbour);
            }
        }
        int tilecount = board.regions
            .Where(obj => obj != null)
            .Select(robj => robj.GetComponent<Region>())
            .Where(com => com != null)
            .SelectMany(region => region.Tiles).Count();
        Debug.Log(tilecount);
        Debug.Assert(tilecount == visited.Count);
        tiles = visited;
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

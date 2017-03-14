using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
[ExecuteInEditMode]
public class HexMesh : MonoBehaviour {

    public const float radiusRatio = 0.866025404f;
    [Header("State/position info")]
    public float radius = 4f;
    public float height = 2f;
    public HexGridCoordinates location;
    [SerializeField]
    private GameObject[] edges = new GameObject[6];



    [Header("Tile generation/Spawning settings")]
    public GameObject hexPrefab;
    public bool spawnFractals = false;
    public GameObject spawnedBy;

    public float OuterRadius { get { return outerRadius; } }
    public Vector3[] OuterVertices { get { return corners; } }
    public GameObject[] Edges { get { return edges; } }
    public HexGridCoordinates Location { get { return location; } set { location = value; } }
    public GameObject SelfPrefab { set { hexPrefab = value; } }

    private List<Vector3> vertices;
    private List<int> triangles;
    private Mesh mesh;
    private LineRenderer lineRenderer;
    private Vector3[] corners;
    private float innerRadius, outerRadius;

    private void Awake()
    {
        outerRadius = radius;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        Generate();
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    private void Generate()
    {
        // WaitForSeconds wait = new WaitForSeconds(0.05f);

        GetComponent<MeshFilter>().sharedMesh = mesh = new Mesh();
        mesh.name = "hexagon";

        innerRadius = outerRadius * radiusRatio;

        Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
        };

        this.corners = corners;

        Vector3 center = new Vector3();
        // Top
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(
                center,
                center + corners[i],
                center + corners[i + 1]
            );
        }
        // Bottom
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(
                center - Vector3.up * height,
                center + corners[i + 1] - Vector3.up * height,
                center + corners[i] - Vector3.up * height
            );
        }

        // Sides
        for (int i = 0;i < 6; i++)
        {
            AddTriangle(
                center + corners[i] - Vector3.up * height,
                center + corners[i + 1],
                center + corners[i]
            );
            AddTriangle(
                center + corners[i] - Vector3.up * height, 
                center + corners[i + 1] - Vector3.up * height, 
                center + corners[i + 1]
            );
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        MeshCollider c = GetComponent<MeshCollider>();
        c.sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }

    public void DrawOutline() {
        if (lineRenderer == null && gameObject.GetComponent<LineRenderer>() == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        else
        {
            if (Application.isPlaying) Destroy(lineRenderer);
            else DestroyImmediate(lineRenderer);
            lineRenderer = gameObject.GetComponent<LineRenderer>();
        }
        lineRenderer.numPositions = corners.Length;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.useWorldSpace = true;
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 point = gameObject.transform.position + corners[i] + new Vector3(0f, 0.1f, 0f);
            lineRenderer.SetPosition(i, point);
        }
    }

    [ContextMenu("Spawn neighbours")]
    public HexGridCoordinates[] SpawnNeighbours() {
        return SpawnNeighboursToDepth(1);
    }

    public HexGridCoordinates[] SpawnNeighboursToDepth(int recursiveDepth) {
        RegionBuilder r = transform.parent.GetComponent<RegionBuilder>();
        HexGridGenerator grid = GameObject.Find("HexTileMom").GetComponent<HexGridGenerator>();
        if (grid == null && r != null)
        {
            grid = transform.parent.parent.GetComponentInChildren<HexGridGenerator>();
        }
        else if (grid == null) grid = GetComponentInParent<HexGridGenerator>();
        if (grid == null) { Debug.LogError("Something went wrong..."); return null; }
        List<HexGridCoordinates> newCoords = spawnNeighboursRecursive(recursiveDepth, r, grid);
        if (r != null)
        {
            IEnumerable<GameObject> newHexObjs = newCoords.Select(coords => grid[coords]);
            if (r.hexTilesToAdd != null)
            {
                r.hexTilesToAdd = r.hexTilesToAdd.Concat(newHexObjs).ToArray();
            }
            else r.hexTilesToAdd = newHexObjs.ToArray();
            r.Consolidate();
        }
        return newCoords.ToArray();
    }

    private List<HexGridCoordinates> spawnNeighboursRecursive(int depth, RegionBuilder r, HexGridGenerator grid) {
        if (depth < 1) return new List<HexGridCoordinates>();
        if (depth > 1) Debug.Log("Spawning at depth " + depth);
        List<HexGridCoordinates> newCoords = new List<HexGridCoordinates>();
        for (int i = 0; i < 6; i++)
        {
            if (Edges[i] == null)
            {
                HexGridCoordinates loc = HexGridGenerator.EdgeToLocation(Location, i);
                if (grid.ContainsTileAt(loc)) continue;
                float horizontalShift = (i == 0 || i == 2) ? innerRadius : (i == 3 || i == 5) ? -innerRadius : (loc.offset - Location.offset) * 2 * innerRadius;
                Vector3 pos = transform.position + new Vector3(horizontalShift, 0, (loc.row - Location.row) * 1.5f * outerRadius);
                if (spawnFractals) hexPrefab.GetComponent<HexMesh>().radius = radius * 0.85f;
                HexMesh hex = Instantiate(hexPrefab, pos, Quaternion.identity).GetComponent<HexMesh>();
                hex.Location = loc;
                hex.gameObject.name = "Hex tile, " + hex.Location.ToString();
                hex.spawnedBy = gameObject;
                hex.SelfPrefab = hexPrefab;
                hex.transform.SetParent(grid.transform);
                hex.DrawOutline();
                grid[loc] = hex.gameObject;
                newCoords.Add(loc);

                for (int j = 0; j < 6; j++)
                {
                    HexGridCoordinates neighbourLoc = HexGridGenerator.EdgeToLocation(loc, j);
                    try
                    {
                        if (!grid.ContainsTileAt(neighbourLoc))
                        {
                            hex.Edges[j] = null;
                            newCoords.AddRange(hex.spawnNeighboursRecursive(depth - 1, r, grid));
                        }
                        else
                        {
                            hex.Edges[j] = grid[neighbourLoc];
                            hex.Edges[j].GetComponent<HexMesh>().Edges[(j + 3) % 6] = hex.gameObject;
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        Debug.Log("Oooook");
                        hex.Edges[j] = null;
                    }
                }
            }
        }
        return newCoords;
    }

    [ContextMenu("Delete from board")]
    public void Delete() {
        HexGridGenerator gen = transform.parent.GetComponent<HexGridGenerator>();
        RegionBuilder r = transform.parent.GetComponent<RegionBuilder>();
        if (r != null && gen != null) Debug.LogError("Wtf that is definitely not supposed to happen");
        else if (r != null) r.ReleaseTile(gameObject);
        else if (gen != null) gen.ReleaseTile(Location);
        else Debug.LogError("Hex tile is not attached to region or generator (" + Location + ")");
        foreach (GameObject edge in edges.Where(edge => edge != null)) {
            HexMesh neighbour = edge.GetComponent<HexMesh>();
            if (neighbour == null) continue;
            for (int i = 0; i < 6; i++)
                if (neighbour.Edges[i] == gameObject)
                {
                    neighbour.Edges[i] = null;
                    break;
                }
        }
        if (Application.isPlaying) Destroy(gameObject);
        else DestroyImmediate(gameObject);
    }

    [ContextMenu("Make region from tile")]
    private void makeRegionFromTile() {
        GameBoard board = transform.parent.GetComponentInParent<GameBoard>();
        if (board == null) throw new System.Exception("The selected tile is not a descendant of a game board.");
        board.CreateRegionWithTiles(new Transform[] { transform });
    }

    void Start()
    {

    }

    void Update()
    {

    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    for (int i = 0; i < OuterVertices.Length - 1; i++)
    //    {
    //        if (i == 0) Gizmos.color = Color.blue;
    //        Gizmos.DrawSphere((transform.position + OuterVertices[i] + Vector3.up), 0.1f);
    //        if (i == 0) Gizmos.color = Color.red;
    //    }
    //}

}

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
    public float height = 100f;
    public float chamfer = 0.075f;
    public HexGridCoordinates location;
    [SerializeField]
    private GameObject[] edges = new GameObject[6];

    [Header("Tile generation/Spawning settings")]
    public GameObject hexPrefab;
    public bool spawnFractals = false;
    public GameObject spawnedBy;
    public bool isWall = false;

    [SerializeField]
    private Vector3[] corners;

    private List<Vector3> vertices;
    private List<int> triangles;
    private int[][] edgeTriangles = Enumerable.Range(0, 6).Select(a => new int[6]).ToArray();
    private Mesh mesh;
    private LineRenderer lineRenderer;
    private float innerRadius, outerRadius;

    public float OuterRadius { get { return outerRadius; } }
    public Vector3[] OuterVertices { get { return corners; } }
    public GameObject[] Edges { get { return edges; } }
    public HexGridCoordinates Location { get { return location; } set { location = value; } }
    public GameObject SelfPrefab { set { hexPrefab = value; } }

    private void Awake()
    {
        height = 100f;
        outerRadius = radius;
        innerRadius = outerRadius * radiusRatio;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        Generate();
    }

    private Vector3[] makeHexVertices(float faceInnerRadius, float faceOuterRadius)
    {
        return new Vector3[]
        {
            new Vector3(0f, 0f, faceOuterRadius),
            new Vector3(faceInnerRadius, 0f, 0.5f * faceOuterRadius),
            new Vector3(faceInnerRadius, 0f, -0.5f * faceOuterRadius),
            new Vector3(0f, 0f, -faceOuterRadius),
            new Vector3(-faceInnerRadius, 0f, -0.5f * faceOuterRadius),
            new Vector3(-faceInnerRadius, 0f, 0.5f * faceOuterRadius),
            new Vector3(0f, 0f, faceOuterRadius)
        };
    }

    private void Generate()
    {
        GetComponent<MeshFilter>().sharedMesh = mesh = new Mesh();
        mesh.name = "hexagon";

        corners = makeHexVertices(innerRadius, outerRadius);
        Vector3[] faceCorners = makeHexVertices(innerRadius - chamfer, outerRadius - chamfer);

        Vector3 center = new Vector3();
        if (isWall) {
            for (int k = 0; k < corners.Length; k++)
                corners[k] += 2 * Vector3.up;
            center += Vector3.up;
            GetComponent<MeshRenderer>().enabled = false;
        }

        vertices.Add(center);
        vertices.AddRange(faceCorners);
        // Top (face)
        for (int i = 0; i < 6; i++)
        {
            triangles.AddRange(new int[] {0, i + 1, 1 + (i + 1) % 6});
        }

        int vertexOffset = vertices.Count;

        // Top outer corners (chamfered)
        vertices.AddRange(corners.Select(v => v + Vector3.down * chamfer));
        // Chamfered edge faces
        for (int i = 0; i < 6; i++)
        {
            Vector3[] quadVertices = new Vector3[]
            {
                faceCorners[i],
                corners[i] + Vector3.down * chamfer,
                corners[(i + 1) % 6] + Vector3.down * chamfer,
                faceCorners[(i + 1) % 6]
            };
            addQuadFace(quadVertices);
        }

        vertexOffset = vertices.Count;
        // Bottom (face)
        vertices.Add(center + Vector3.down * height);
        vertices.AddRange(corners.Select(v => v + Vector3.down * height));
        for (int i = 0; i < 6; i++)
        {
            triangles.AddRange(new int[] {vertexOffset, vertexOffset + 1 + (i + 1) % 6, vertexOffset + 1 + i});
        }

        // Sides
        for (int i = 0; i < 6; i++)
        {
            Vector3[] quadVertices = new Vector3[]
            {
                corners[i] + Vector3.down * chamfer,
                corners[i] + Vector3.down * height,
                corners[(i + 1) % 6] + Vector3.down * height,
                corners[(i + 1) % 6] + Vector3.down * chamfer,
            };
            addQuadFace(quadVertices);
        }

        mesh.vertices = vertices.ToArray();
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        MeshCollider c = GetComponent<MeshCollider>();
        c.sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }

    private void addQuadFace(Vector3[] quadVertices)
    {
        if (quadVertices.Length != 4) throw new System.Exception("Face must be a quad.");
        int start = vertices.Count;
        vertices.AddRange(quadVertices);
        triangles.AddRange(Enumerable.Range(start, 3));
        triangles.AddRange(new int[] { start + 2, start + 3, start });
    }

    [ContextMenu("Refresh outline")]
    public void DrawOutline() {
        if (isWall) return;
        if (lineRenderer != null || gameObject.GetComponent<LineRenderer>() != null)
            if (Application.isPlaying) Destroy(lineRenderer);
            else DestroyImmediate(lineRenderer);
        lineRenderer = gameObject.AddComponent<LineRenderer>();
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

    [ContextMenu("Spawn neighbours to depth 3")]
    private HexGridCoordinates[] SpawnNeighboursOfNeighboursOfNeighbours() { return SpawnNeighboursToDepth(3); }

    [ContextMenu("Spawn neighbours")]
    private void SpawnNeighbours() { SpawnNeighboursToDepth(1); }
    
    public HexGridCoordinates[] SpawnNeighboursToDepth(int recursiveDepth = 1) {
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
                HexMesh hex = grid.makeTile(loc);
                newCoords.Add(loc);
                HexGridCoordinates neighbourLoc;
                for (int j = 0; j < 6; j++)
                {
                    neighbourLoc = HexGridGenerator.EdgeToLocation(loc, j);
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
                        Debug.Log("Oooook that's not good");
                        hex.Edges[j] = null;
                    }
                }
            }
        }
        return newCoords;
    }

    [ContextMenu("Delete from board")]
    public void Delete() {
        HexGridGenerator gen;
        RegionBuilder r = transform.parent.GetComponent<RegionBuilder>();
        if (r != null)
        {
            r.ReleaseTile(gameObject);
            gen = transform.parent.GetComponentInChildren<HexGridGenerator>();
        }
        else gen = transform.parent.GetComponent<HexGridGenerator>();

        if (gen != null) gen.ReleaseTile(Location);
        else Debug.LogError("Hex tile is not attached to a generator: " + Location);
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
[ExecuteInEditMode]
public class HexMesh : MonoBehaviour {

    public const float radiusRatio = 0.866025404f;
    public float radius = 4f;
    public HexTileLocation location;

    public float OuterRadius { get { return outerRadius; } }
    public Vector3[] OuterVertices { get { return corners; } }
    public GameObject[] Edges { get { return edges; } }
    public HexTileLocation Location { get { return location; } set { location = value; } }

    [SerializeField]
    private GameObject[] edges = new GameObject[6];

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
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(
                center,
                center + corners[i],
                center + corners[i + 1]
            );
        }
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(
                center - Vector3.up,
                center + corners[i + 1] - Vector3.up,
                center + corners[i] - Vector3.up
            );
        }

        for (int i = 0;i < 6; i++)
        {
            AddTriangle(
                center + corners[i] - Vector3.up,
                center + corners[i + 1],
                center + corners[i]
            );
            AddTriangle(
                center + corners[i] - Vector3.up, 
                center + corners[i + 1] - Vector3.up, 
                center + corners[i + 1]
            );
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
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

    public void Delete() {
        HexGridGenerator gen = transform.parent.GetComponent<HexGridGenerator>();
        if (gen != null) gen.ReleaseTile(Location.row, Location.offset);
        Region r = transform.parent.GetComponent<Region>();
        if (r != null) r.ReleaseTile(gameObject);
        foreach (GameObject edge in edges) {
            HexMesh neighbour = edge.GetComponent<HexMesh>();
            if (neighbour == null) continue;
            for (int i = 0; i < 6; i++)
                if (neighbour.Edges[i] == gameObject)
                {
                    neighbour.Edges[i] = null;
                    break;
                }
        }
        Destroy(gameObject);
    }

    [ContextMenu("Make region from tile")]
    private void makeRegionFromTile() {
        GameBoard board = transform.parent.GetComponentInParent<GameBoard>();
        if (board == null) throw new System.Exception("The selected tile is not a descendant of a game board.");
        board.CreateRegionWithTiles(new Transform[] { transform });
    }

    [UnityEditor.MenuItem("HexTiles/Make region from selected tiles")]
    private static void makeRegionFromSelectedTiles()
    {
        IEnumerable<Transform> hexes = UnityEditor.Selection.GetTransforms(UnityEditor.SelectionMode.Editable)
            .Where(t => t.gameObject != null)
            .Where(t => t.GetComponent<HexMesh>() != null);
        GameBoard board = FindObjectOfType<GameBoard>();
        if (board == null) throw new System.Exception("Could not find a GameBoard.");
        board.CreateRegionWithTiles(hexes);
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

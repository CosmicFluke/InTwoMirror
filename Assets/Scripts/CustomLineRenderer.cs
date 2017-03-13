using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLineRenderer : MonoBehaviour {

    public Material material;
    
    /// <summary> Used if no material is given.</summary>
    public Shader shader;
    /// <summary> Used if no material is given.</summary>
    public Color lineColor = new Color(0, 240, 120, 0.3f);
    [Range(0.05f, 1.0f)]
    public float lineSize = 0.2f;

    /// <summary>A surface to conform the mesh to.  Must cover entire area of this mesh in x-z plane.</summary>
    [Header("Conform mesh height to terrain/surface")]
    [Tooltip("(Optional) A surface to conform the mesh to.  Must cover entire area of this mesh in x-z plane.")]
    public GameObject terrainObject;
    [Range(0.1f, 2.0f), Tooltip("Maximum distance along y-axis for conforming to a surface; will throw error if distance is exceeded.")]
    public float surfaceConformMaxDistance = 1.0f;

    /// <summary>
    /// Stores the points used to construct the line segments for the mesh.
    /// Every two consecutive points in the list are a single line segment. <para/>
    /// List should always have an even number of elements.
    /// </summary>
    [SerializeField]
    private List<Vector3> lineSegmentPoints = new List<Vector3>();
    [SerializeField]
    private bool isInitialized = false;

    public int LineCount { get { return lineSegmentPoints.Count / 2;  } }
    private Mesh mesh;

    void Awake() {
        if (!isInitialized)
            Init();
        draw();
    }

    /// <summary>
    /// Initialize the data structures.
    /// </summary>
    public void Init() {
        if (lineSegmentPoints == null) lineSegmentPoints = new List<Vector3>();
        rebuildMesh();
        checkMaterial();
        isInitialized = true;
    }

    [ContextMenu("Force draw")]
    public void Draw()
    {
        if (!isInitialized)
            Init();
        draw();
    }

    void Update()
    {
        draw();
    }

    /// <summary>
    /// Add a line to the mesh
    /// </summary>
    /// <param name="s"></param>
    /// <param name="e"></param>
    public void AddLine(Vector3 s, Vector3 e) {
        if (!isInitialized)
            Init();
        addLine(mesh, makeQuad(transform.TransformPoint(s), e, lineSize), false);
        lineSegmentPoints.Add(s);
        lineSegmentPoints.Add(e);
    }

    [ContextMenu("Conform mesh to surface")]
    public void ConformToSurface() {
        conformToSurface(terrainObject, surfaceConformMaxDistance);
    }

    [ContextMenu("Reset mesh height (uniform)")]
    public void ResetMeshHeight()
    {
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 vertex = mesh.vertices[i];
            vertex.y = transform.position.y;
            mesh.vertices[i] = vertex;
        }
    }

    /// <summary>
    /// Draws the mesh in world space
    /// </summary>
    void draw()
    {
        Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, 0);
    }

    void rebuildMesh()
    {
        if (lineSegmentPoints.Count % 2 != 0)
            throw new System.Exception("Line segments list should contain an even number of points (2 for each line).");
        if (mesh != null)
            Destroy(mesh);
        mesh = new Mesh();
        for (int i = 0; i < lineSegmentPoints.Count; i += 2)
            AddLine(lineSegmentPoints[i], lineSegmentPoints[i + 1]);
    }

    void conformToSurface(GameObject obj, float maxDistance)
    {
        if (obj == null)
            throw new System.Exception("No surface object provided.");
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider == null && (colliders == null || colliders.Length == 0))
            throw new System.Exception("Provided surface object does not have a collider component.");
        if (objCollider != null)
            colliders = new Collider[] { objCollider };

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 vertex = mesh.vertices[i];
            RaycastHit hit;
            Ray ray = new Ray(new Vector3(vertex.x, vertex.y + maxDistance, vertex.z), Vector3.down);
            if (Physics.Raycast(ray, out hit, 2.0f * maxDistance, LayerMask.NameToLayer("Terrain")))
            {
                Debug.Log("Hit point: " + hit.point);
                vertex.y = hit.point.y;
                mesh.vertices[i] = vertex;
            }
            else throw new System.Exception("Terrain is not within maxDistance (" + maxDistance.ToString() +
                ") of hex grid at " + new Vector2(vertex.x, vertex.y).ToString());
        }
    }

    /// <summary>
    /// Uses starting and ending points to produce the corner vectors for a quad.<para/>
    /// Taken from code provided in an Everyday3D blog post
    /// <para/>
    /// http://www.everyday3d.com/blog/index.php/2010/03/15/3-ways-to-draw-3d-lines-in-unity3d/
    /// </summary>
    /// <param name="s">Starting vector</param>
    /// <param name="e">Ending vector</param>
    /// <param name="w">Line width</param>
    Vector3[] makeQuad(Vector3 s, Vector3 e, float w)
    {
        w = w / 2;
        Vector3[] q = new Vector3[4];

        Vector3 n = Vector3.Cross(s, e);
        Vector3 l = Vector3.Cross(n, e - s);
        l.Normalize();

        q[0] = transform.InverseTransformPoint(s + l * w);
        q[1] = transform.InverseTransformPoint(s + l * -w);
        q[2] = transform.InverseTransformPoint(e + l * w);
        q[3] = transform.InverseTransformPoint(e + l * -w);

        return q;
    }

    /// <summary>
    /// Adds a line segment (given as corner vectors for a quad) to the mesh<para/>
    /// Taken from code provided in an Everyday3D blog post
    /// <para/>
    /// http://www.everyday3d.com/blog/index.php/2010/03/15/3-ways-to-draw-3d-lines-in-unity3d/
    /// </summary>
    /// <param name="m">Mesh to be modified</param>
    /// <param name="quad">The corner vectors</param>
    /// <param name="tmp">Is the line temporary?</param>
    void addLine(Mesh m, Vector3[] quad, bool tmp)
    {
        int vl = m.vertices.Length;

        Vector3[] vs = m.vertices;
        if (!tmp || vl == 0) vs = resizeVertices(vs, 4);
        else vl -= 4;

        vs[vl] = quad[0];
        vs[vl + 1] = quad[1];
        vs[vl + 2] = quad[2];
        vs[vl + 3] = quad[3];

        int tl = m.triangles.Length;

        int[] ts = m.triangles;
        if (!tmp || tl == 0) ts = resizeTraingles(ts, 6);
        else tl -= 6;
        ts[tl] = vl;
        ts[tl + 1] = vl + 1;
        ts[tl + 2] = vl + 2;
        ts[tl + 3] = vl + 1;
        ts[tl + 4] = vl + 3;
        ts[tl + 5] = vl + 2;

        m.vertices = vs;
        m.triangles = ts;
        m.RecalculateBounds();
    }

    Vector3[] resizeVertices(Vector3[] ovs, int ns)
    {
        Vector3[] nvs = new Vector3[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
    }

    int[] resizeTraingles(int[] ovs, int ns)
    {
        int[] nvs = new int[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
    }

    void checkMaterial()
    {
        if (material == null)
        {
            material = new Material(shader);
            material.color = lineColor;
        }
    }

    public Material Material { get { return material; } set { material = value; checkMaterial(); } }
}

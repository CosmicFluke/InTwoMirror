using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public class FlatHex : MonoBehaviour {

    public const float radiusRatio = 0.866025404f;
    public float radius = 5f;
    private Mesh mesh;

    [SerializeField]
    private Vector3[] corners;

    private List<Vector3> vertices;
    private List<int> triangles;
    private float innerRadius, outerRadius;

    public float OuterRadius { get { return outerRadius; } }
    public Vector3[] OuterVertices { get { return corners; } }

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

        Vector3 center = new Vector3();

        vertices.Add(center);
        vertices.AddRange(corners);
        List<Vector2> uv = new List<Vector2> { new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0), new Vector2(1f, 0.25f), new Vector2(1f, 0.75f), new Vector2(0.5f, 1f), new Vector2(0, 0.75f), new Vector2(0, 0.25f), new Vector2(0.5f, 0) };
        // Top (face)
        for (int i = 0; i < 6; i++)
        {
            triangles.AddRange(new int[] { 0, i + 1, 1 + (i + 1) % 6 });
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uv.ToArray();
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
    }

    // Use this for initialization
    void Awake () {
        outerRadius = radius;
        innerRadius = outerRadius * radiusRatio;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        Generate();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

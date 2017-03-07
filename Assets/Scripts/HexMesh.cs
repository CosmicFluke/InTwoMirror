using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    private List<Vector3> vertices;
    private Vector3[] v;
    private List<int> triangles;
    private Mesh mesh;

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
        WaitForSeconds wait = new WaitForSeconds(0.05f);

        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "hexagon";

        float outerRadius = 5f;

        float innerRadius = outerRadius * 0.866025404f;

        Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
        };

        Vector3 center = transform.localPosition;
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(
                center,
                center + corners[i],
                center + corners[i + 1]
            );
        }


        mesh.vertices = vertices.ToArray();
        v = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private void Awake()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        Generate();
    }

    void Start()
    {

    }

    void Update()
    {

    }

    /*
    private void OnDrawGizmos()
    {
        if (v == null)
        {
            return;
        }
        Gizmos.color = Color.black;
        for (int i = 0; i < v.Length; i++)
        {
            Gizmos.DrawSphere(v[i], 0.1f);
        }
    }
    */

}

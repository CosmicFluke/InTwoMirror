using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public enum RegionState { A, B, C }
public enum RegionEffect { Stable, Unstable, Volatile }
public enum Actions { Shift, Flip,  }

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Region : MonoBehaviour {

    public static RegionEffect StateToEffect(RegionState state, PlayerID player)
    {
        if (state == RegionState.C) return RegionEffect.Volatile;
        else return player == PlayerID.P1 ? (RegionEffect)state : (RegionEffect)(((int)state - 1) * -1);
    }

    public RegionState initialState = RegionState.A;
    public GameObject[] neighbouringRegions;
    public string RegionID;
    public Material outlineMaterial;

    public RegionState State { get { return this.currentState; } }

    private RegionState currentState;
    private bool isMerged = false;
    private LineRenderer outline;

    void Start () {
        currentState = initialState;
	}

    public void ShiftState(int offset) {
        currentState = (RegionState)(((int)currentState + offset) % 3);
    }

    public void MakeRegionFromTiles(GameObject[] hexTiles) {
        MeshFilter[] toMerge = hexTiles.Select(tile => tile.GetComponent<MeshFilter>()).ToArray();
        mergeMeshes(toMerge);
    }

    public void FlattenChildMeshes() {
        MeshFilter[] toMerge = GetComponentsInChildren<MeshFilter>();
        mergeMeshes(toMerge);
    }

    /**
     * Merge an array of meshes into a single mesh and add it to this object's MeshFilter
     */
    private void mergeMeshes(MeshFilter[] meshFilters) {
        if (isMerged)
        {
            Debug.Log("Sub-meshes for region \"" + RegionID + "\" are already merged.");
            return;
        }
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }
        Mesh newMesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh = newMesh;
        newMesh.CombineMeshes(combine);
        isMerged = true;
        // renderLine();  // buggy
    }

    /** 
     * Renders a border around the region
     * Currently non-working
     */
    private LineRenderer renderLine()
    {
        Vector3[] corners = findOutlinePath(GetComponent<MeshFilter>().sharedMesh);
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.numPositions = corners.Length + 1;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = outlineMaterial;
        lineRenderer.transform.parent = gameObject.transform;
        for (int i = 0; i < 7; i++)
        {
            Vector3 point = 2 * gameObject.transform.position + corners[i] + new Vector3(0f, 0.1f, 0f);
            lineRenderer.SetPosition(i, point);
        }
        return lineRenderer;
    }

    private Vector3[] findOutlinePath(Mesh mesh) {
        HashSet<int> visitedVertices = new HashSet<int>();
        List<int> path = new List<int>();
        Dictionary<int, VertexContext> contextMemo = new Dictionary<int, VertexContext>();
        VertexContext context;
        int currVertex = mesh.triangles[0];
        contextMemo[currVertex] = getContext(mesh, currVertex);
        while (currVertex != mesh.triangles[0] || visitedVertices.Count == 0)
        {
            visitedVertices.Add(currVertex);
            context = contextMemo[currVertex];
            Stack<int> edges = new Stack<int>(context.edges.Except(visitedVertices));
            Debug.Log("Stack has " + edges.Count.ToString() + " items");
            int nextVertex = edges.Pop();
            if (context.triangles.Length != 6)
            {
                path.Add(currVertex);
                if (!contextMemo.ContainsKey(nextVertex)) contextMemo[nextVertex] = getContext(mesh, nextVertex);
                while (contextMemo[currVertex].triangles.Intersect(contextMemo[nextVertex].triangles).Count() > 1)
                {
                    if (edges.Count == 0) throw new Exception("Empty stack.  Something went wrong!");
                    nextVertex = edges.Pop();
                    if (!contextMemo.ContainsKey(nextVertex)) contextMemo[nextVertex] = getContext(mesh, nextVertex);
                }
            }
            currVertex = nextVertex;
        }
        path.Add(currVertex);
        return path.ToArray().Select(i => mesh.vertices[i]).ToArray();
    }

    private VertexContext getContext(Mesh mesh, int vertex)
    {
        int numTriangles = 0;
        HashSet<int> connectedVertices = new HashSet<int>();
        List<int> triangles = new List<int>();
        connectedVertices.Add(vertex);
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Triangle triangle = new Triangle(mesh.triangles, i);
            if (triangle.vertices.Contains(vertex))
            {
                triangles.Add(i / 3);
                connectedVertices.UnionWith(triangle.vertices);
            }
        }
        connectedVertices.Remove(vertex);
        VertexContext t;
        t.edges = connectedVertices.ToArray();
        t.triangles = triangles.ToArray();
        return t;
    }

    private struct Triangle
    {
        public int[] vertices;
        public Triangle(int v1, int v2, int v3)
        {
            vertices = new int[] { v1, v2, v3 };
            if (vertices.Distinct().Count() != 3) throw new Exception("Not all diff.");
        }
        public Triangle(int[] vertices, int index) {
            this.vertices = new int[] { vertices[index], vertices[index + 1], vertices[index + 2] };
            if (this.vertices.Distinct().Count() != 3) throw new Exception("Not all diff.");
        }
    }

    private struct VertexContext {
        public int[] edges;
        public int[] triangles;
    }
}

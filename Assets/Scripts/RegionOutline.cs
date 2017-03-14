using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(Region))]
public class RegionOutline : MonoBehaviour {

    public Material Material {
        get { return material; }
        set
        {
            material = value;
            LineRenderer outline = GetComponent<LineRenderer>();
            outline.material = material;
        }
    }

    public bool IsActive;

    public Material material;
    [Range(0.05f, 1.0f)]
    public float lineBaseSize = 0.1f;

    /// <summary> Used if no material is given.</summary>
    public Color lineColor = Color.white;

    /// <summary>A surface to conform the mesh to.  Must cover entire area of this mesh in x-z plane.</summary>
    [Header("Conform mesh height to terrain/surface")]
    [Tooltip("(Optional) A surface to conform the mesh to.  Must cover entire area of this mesh in x-z plane.")]
    public GameObject terrainObject;
    [Range(0.1f, 5.0f), Tooltip("Maximum distance along y-axis for conforming to a surface; will throw error if distance is exceeded.")]
    public float surfaceConformMaxDistance = 5.0f;
    public Vector3[] vertices;

    private float currLineSize;
    private float growRate = 1.0f; // number of cycles per second
    private float growFactor = 2.0f; //
    private bool isGrowing;

    // Use this for initialization
    void Start () {
        currLineSize = lineBaseSize;
	}
	
	// Update is called once per frame
	void Update () {
        if (IsActive)
        {
            float deltaSize = (growRate * Time.deltaTime) * growFactor * lineBaseSize;
            currLineSize += (isGrowing ? 1 : -1) * deltaSize;
            if (currLineSize <= lineBaseSize)
                isGrowing = true;
            else if (currLineSize >= lineBaseSize + growFactor * lineBaseSize)
                isGrowing = false;
            refreshLineSize();
        }
        else if (currLineSize != lineBaseSize)
        {
            currLineSize = lineBaseSize;
            refreshLineSize();
        }
	}

    private void refreshLineSize()
    {
        LineRenderer outline = GetComponent<LineRenderer>();
        outline.startWidth = currLineSize;
        outline.endWidth = currLineSize;
    }

    public void Refresh()
    {
        LineRenderer outline = GetComponent<LineRenderer>();
        outline.material = Material;
        outline.startWidth = lineBaseSize;
        outline.endWidth = lineBaseSize;
        if (outline.sharedMaterial == null)
        {
            outline.startColor = lineColor;
            outline.endColor = lineColor;
        }

        outline.numPositions = vertices.Length;
        outline.SetPositions(vertices.Select(v => v + transform.position + Vector3.up * lineBaseSize / 2f).ToArray());
    }

    public Vector3[] Vertices {
        get { return vertices.ToArray(); }
        set {
            vertices = value.ToArray();
            Debug.Log(string.Join(" / ", vertices.Select(v => v.ToString()).ToArray()));
            Refresh();
        }
    } 

    [ContextMenu("Conform mesh to surface (doesn't work)")]
    public void ConformToSurface()
    {
        conformToSurface(terrainObject, surfaceConformMaxDistance);
    }

    [ContextMenu("Reset mesh height (uniform) (doesn't work)")]
    public void ResetVertexHeight()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        for (int i = 0; i < lr.numPositions; i++)
        {
            Vector3 vertex = lr.GetPosition(i);
            vertex.y = transform.position.y;
            lr.SetPosition(i, vertex);
        }
    }

    void conformToSurface(GameObject obj, float maxDistance)
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        if (obj == null)
            throw new System.Exception("No surface object provided.");
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider == null && (colliders == null || colliders.Length == 0))
            throw new System.Exception("Provided surface object does not have a collider component.");
        if (objCollider != null)
            colliders = new Collider[] { objCollider };
        Vector3 vertex;

        for (int i = 0; i < lr.numPositions; i++)
        {
            vertex = lr.GetPosition(i);
            Vector3 source = new Vector3(vertex.x, vertex.y + maxDistance / 2, vertex.z);
            Debug.Log("Casting from: " + source.ToString());
            RaycastHit hit;
            Debug.DrawRay(source, Vector3.down * 10, Color.red, 20.0f);
            Ray ray = new Ray(source, Vector3.down);
            if (Physics.Raycast(ray, out hit, 2.0f * maxDistance, LayerMask.NameToLayer("Terrain")))
            {
                Debug.Log("Hit point: " + hit.point);
                vertex.y = hit.point.y;
                lr.SetPosition(i, vertex);
            }
            else
            {
                Debug.Log("Terrain is not within maxDistance (" + maxDistance.ToString() +
                ") of hex grid at " + new Vector2(vertex.x, vertex.y).ToString());   
            }
        }
    }
}

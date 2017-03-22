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

    public bool IsActive { get { return isActive; } set { isActive = value; } }
    public bool isActive; // mirrored in property for setter debugging
    private bool[] neighbourActive = new bool[] { false, false };

    public Material material;
    [Range(0.05f, 1)]
    public float baseLineSize = 0.1f;
    [Range(0.5f, 5)]
    public float initialGrowRate = 1.0f; // Number of pulses per second when pulse is active
    [Range(0.5f, 5)]
    public float initialGrowFactor = 2.0f; // Amount to grow (* lineBaseSize) when pulse is active

    /// <summary> Used if no material is given.</summary>
    public Color lineColor = Color.white;

    /// <summary>A surface to conform the mesh to.  Must cover entire area of this mesh in x-z plane.</summary>
    [Header("Conform mesh height to terrain/surface")]
    [Tooltip("(Optional) A surface to conform the mesh to.  Must cover entire area of this mesh in x-z plane.")]
    public GameObject terrainObject;
    [Range(0.1f, 5.0f), Tooltip("Maximum distance along y-axis for conforming to a surface; will throw error if distance is exceeded.")]
    public float surfaceConformMaxDistance = 5.0f;
    public Vector3[] vertices;

    private float lineSize;
    private float growRate;
    private float growFactor;
    private bool isGrowing = false;
    private Material bothMaterial;

    // Use this for initialization
    void Start () {
        GameBoard board = GetComponentInParent<GameBoard>();
        lineSize = baseLineSize;
        ResetPulse();
        float h, s, v;
        bothMaterial = Instantiate(Material);
        Color between = Color.Lerp(board.OutlineMaterials[0].color, board.OutlineMaterials[1].color, 0.5f);
        Color.RGBToHSV(between, out h, out s, out v);
        bothMaterial.color = Color.HSVToRGB(h, s * 1.1f, v * 1.1f);
    }
	
	// Update is called once per frame
	void Update () {
        if (IsActive)
        {
            float deltaSize = (growRate * Time.deltaTime) * growFactor * baseLineSize;
            lineSize += (isGrowing ? 1 : -1) * deltaSize;
            if (lineSize <= baseLineSize)
                isGrowing = true;
            else if (lineSize >= baseLineSize + growFactor * baseLineSize)
                isGrowing = false;
            refreshLineSize();
        }
        else if (lineSize != baseLineSize)
        {
            lineSize = baseLineSize;
            refreshLineSize();
        }
	}

    public void EnhancePulse(float growRate, float growFactor)
    {
        EnhancePulse(growRate, growFactor, Color.red);
    }

    public void EnhancePulse(float growRate, float growFactor, Color newLineColor)
    {
        LineRenderer outline = GetComponent<LineRenderer>();
        outline.startColor = outline.endColor = newLineColor;
        this.growRate = growRate;
        this.growFactor = growFactor;
    }

    public void ResetPulse() {
        LineRenderer outline = GetComponent<LineRenderer>();
        outline.startColor = outline.endColor = lineColor;
        growFactor = initialGrowFactor;
        growRate = initialGrowRate;
    }

    public void setActive(bool value, PlayerID player)
    {
        neighbourActive[(int)player] = value;
        isActive = neighbourActive[0] || neighbourActive[1];
        setMaterial();
    }

    private void setMaterial()
    {
        if (neighbourActive[0] && neighbourActive[1])
            Material = bothMaterial;
        else if (neighbourActive[0])
            Material = GetComponentInParent<GameBoard>().OutlineMaterials[1];
        else if (neighbourActive[1])
            Material = GetComponentInParent<GameBoard>().OutlineMaterials[0];
        else Material = GetComponent<Region>().OutlineMaterial;
    }

    private void refreshLineSize()
    {
        LineRenderer outline = GetComponent<LineRenderer>();
        outline.startWidth = lineSize;
        outline.endWidth = lineSize;
    }

    public void Refresh()
    {
        LineRenderer outline = GetComponent<LineRenderer>();
        outline.material = Material;
        outline.startWidth = baseLineSize;
        outline.endWidth = baseLineSize;
        if (outline.sharedMaterial == null)
        {
            outline.startColor = lineColor;
            outline.endColor = lineColor;
        }

        outline.numPositions = vertices.Length;
        outline.SetPositions(vertices.Select(v => transform.rotation * v + transform.position + Vector3.up * baseLineSize / 2f).ToArray());
    }

    public Vector3[] Vertices {
        get { return vertices.ToArray(); }
        set {
            vertices = value.ToArray();
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

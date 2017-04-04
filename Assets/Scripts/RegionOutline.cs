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
    public float baseLineSize = 0.25f;
    [Range(0.5f, 5)]
    public float initialGrowRate = 0f; // Number of pulses per second when pulse is active
    [Range(0.5f, 5)]
    public float initialGrowFactor = 0f; // Amount to grow (* lineBaseSize) when pulse is active

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

    private Mesh mesh;

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
        Refresh();
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
        if (!isActive) ResetPulse();
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
        Debug.Log("Refreshing outline");
        LineRenderer outline = GetComponent<LineRenderer>();
        outline.material = Material;
        outline.startWidth = baseLineSize;
        outline.endWidth = baseLineSize;
        if (outline.sharedMaterial == null)
        {
            outline.startColor = lineColor;
            outline.endColor = lineColor;
        }

        vertices = getBorderVertices(baseLineSize / 2f).ToArray();
        outline.numPositions = vertices.Length;
        outline.SetPositions(vertices.Select(v => transform.rotation * v + transform.position + Vector3.up * 0.05f).ToArray());
    }

    public Vector3[] Vertices {
        get { return vertices.ToArray(); }
        set {
            vertices = value.ToArray();
            Refresh();
        }
    }

    private List<Vector3> getBorderVertices(float insideBorder)
    {
        insideBorder *= 0.95f;
        GameObject[] tiles = GetComponent<Region>().Tiles.ToArray();
        int tileNum = 0;
        GameObject startingTile = null;
        int startingEdge = -1;
        while (startingEdge < 0)
        {
            if (tileNum >= tiles.Length) throw new System.Exception("Could not find a suitable outer tile for the region. Data error.");
            startingTile = tiles[tileNum];
            tileNum++;
            startingEdge = findOuterEdge(startingTile.GetComponent<HexMesh>());
        }

        List<Vector3> vertices = new List<Vector3>();
        GameObject currTile = startingTile;
        GameObject nextTile = null;
        int currEdge = startingEdge;
        int nextEdge = -1;
        bool concaveVertex = false;
        HexMesh hex = currTile.GetComponent<HexMesh>();
        Vector3 vertex = hex.transform.position - transform.position + hex.OuterVertices[currEdge];
        vertices.Add(shiftPointInsideRegion(vertex, hex, currEdge, concaveVertex, insideBorder));
        // Advance around hex edges in a clockwise direction
        while (currTile != startingTile || currEdge != startingEdge || vertices.Count == 1)
        {
            nextTile = currTile;
            nextEdge = (currEdge + 1) % 6;
            if (tiles.Contains(hex.Edges[nextEdge]))
            {
                vertex = hex.transform.position - transform.position + hex.OuterVertices[(currEdge + 1) % 6];
                vertices.Add(shiftPointInsideRegion(vertex, hex, currEdge, concaveVertex, insideBorder));
                concaveVertex = true;
                nextTile = hex.Edges[nextEdge];
                nextEdge = (nextEdge + 4) % 6;
            }
            else concaveVertex = false;
            vertex = hex.transform.position - transform.position + hex.OuterVertices[(currEdge + 1) % 6];
            vertices.Add(shiftPointInsideRegion(vertex, hex, currEdge, concaveVertex, insideBorder));
            // Advance to the next edge on the hex
            currEdge = nextEdge;
            currTile = nextTile;
            hex = currTile.GetComponent<HexMesh>();
        }
        return vertices;
    }

    private int findOuterEdge(HexMesh hex)
    {
        GameObject[] tiles = GetComponent<Region>().Tiles.ToArray();
        for (int i = 0; i < 6; i++)
        {
            if (hex.Edges[i] == null) continue;
            if (!tiles.Contains(hex.Edges[i]))
                return i;
        }
        return -1;
    }

    private Vector3 shiftPointInsideRegion(Vector3 vertex, HexMesh hex, int currEdge, bool concaveVertex, float amt)
    {
        Vector3 shiftDirection = hex.transform.position - transform.position + (concaveVertex ? hex.OuterVertices[(currEdge + 2) % 6] : Vector3.zero);
        return Vector3.MoveTowards(vertex, shiftDirection, 0.5f * amt / Mathf.Cos(Mathf.Deg2Rad * 30));
    }
}

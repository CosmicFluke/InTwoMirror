using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RegionState { A, B, C }
public enum RegionEffect { Stable, Unstable, Volatile }
public enum Action { Useless, Swap, Shift }

public abstract class Region : MonoBehaviour {

    public static RegionEffect StateToEffect(RegionState state, PlayerID player)
    {
        if (state == RegionState.C) return RegionEffect.Volatile;
        else return player == PlayerID.P1 ? (RegionEffect)state : (RegionEffect)(((int)state - 1) * -1);
    }

    public IEnumerable<GameObject> Neighbours { get { return neighbours.AsEnumerable(); } }

    public RegionState State
    {
        get { return currentState; }
        set
        {
            currentState = value;
            updateMaterials();
        }
    }

    [Header("Set-up properties")]
    public RegionState initialState = RegionState.A;
    [SerializeField]
    protected GameObject[] neighbours;
    [SerializeField]
    protected List<GameObject> hexTiles = new List<GameObject>();
    [SerializeField]
    protected Material[] tileMaterials = new Material[3];
    [SerializeField]
    protected Material[] outlineMaterials = new Material[3];

    [Header("Runtime properties")]
    public RegionState currentState;
    public Material material;

    public void PropagateAction(Action action, PlayerID player, int distance) {
        State = ActionDictionary.Lookup(action, State, player);
        if (distance == -1)
            distance = GetComponentInParent<GameBoard>().StateChangePropagationDistance;
        else if (distance <= 1) return;
        foreach (Region r in Neighbours.Select(o => o.GetComponent<Region>()))
        {
            r.PropagateAction(action, player, distance - 1);
        }
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetOccupied(bool isActive, PlayerID player)
    {
        if (isActive) {
            GetComponent<RegionOutline>().IsActive = true;
        }
        else
        {
            GetComponent<RegionOutline>().IsActive = false;
        }
    }

    public void ShiftState(int offset)
    {
        State = (RegionState)(((int)State + offset) % 3);
    }

    protected void refresh() {
        refreshColliders();
        GetComponent<RegionOutline>().Rebuild();
        updateMaterials();
    }

    /// <summary>
    /// Helper method to ensure the current material is active on the region tile(s)
    /// </summary>
    [ContextMenu("Update materials")]
    protected void updateMaterials()
    {
        if (tileMaterials != null && tileMaterials[(int)State] != null)
            foreach (GameObject tile in hexTiles)
                tile.transform.GetComponent<MeshRenderer>().material = tileMaterials[(int)State];
        RegionOutline outline = GetComponent<RegionOutline>();
        if (outline != null && outlineMaterials != null && outlineMaterials[(int)State] != null)
        {
            outline.Material = outlineMaterials[(int)State];
        }
    }

    /// <summary>
    /// Resets the colliders -- to be used only if the colliders are out of place after an adjustment or 
    /// instantiation, or if the region composition changes (when building a board).
    /// </summary>
    protected void refreshColliders()
    {
        Collider[] colliders = GetComponents<Collider>();
        if (colliders != null)
            foreach (Collider c in colliders)
                if (Application.isPlaying) Destroy(c);
                else DestroyImmediate(c);

        foreach (GameObject tile in hexTiles)
        {
            Mesh oldMesh = tile.GetComponent<MeshFilter>().sharedMesh;
            MeshCollider mc = gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = new Mesh();
            mc.sharedMesh.vertices = oldMesh.vertices
                .Select(v => tile.transform.TransformPoint(v) - transform.position + 2.0f * Vector3.up)
                .ToArray();
            mc.sharedMesh.triangles = oldMesh.triangles;
            mc.convex = true;
            mc.isTrigger = true;
        }
    }
}

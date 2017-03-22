using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RegionState { A, B, C }
public enum RegionEffect { Stable, Unstable, Volatile }

public class Region : MonoBehaviour {

    [Header("Set-up properties")]
    public RegionState initialState;
    [Range(1, 5)] public float volatileDurationUntilDeath = 2f;
    [Range(1, 10)] public float initialDmgRate = 1f;
    [SerializeField]
    protected GameObject[] neighbours;
    [SerializeField]
    protected List<GameObject> hexTiles = new List<GameObject>();
    [SerializeField]
    protected Material[] tileMaterials = new Material[3];
    [SerializeField]
    protected Material[] outlineMaterials = new Material[3];

    protected RegionState currentState;

    public bool IsGoal { get; set; }

    // ax^2 + bx + c = 0

    // null if no player is occupying the region
    Transform currentPlayer = null;
    // currentEffect is only used when the tile is occupied
    private RegionEffect currentEffect;
    private bool ready = false;
    // used only when the region is occupied and the tile effect is Volatile
    float volatileTimer;
    float prevTime;

    public IEnumerable<GameObject> Tiles { get { return hexTiles.AsEnumerable(); } }
    public IEnumerable<GameObject> Neighbours { get { return neighbours.AsEnumerable(); } }
    public GameObject this[int i] { get { return hexTiles[i]; } }
    public bool IsOccupied { get { return currentPlayer != null; } }
    public Transform CurrentPlayer { get { return currentPlayer; } }
    public Material OutlineMaterial { get { return outlineMaterials[(int)currentState]; } }
    public Material TileMaterial { get { return tileMaterials[(int)currentState]; } }
    public bool IsReady { get { return ready; } }

    public RegionState State
    {
        get { return currentState; }
        set
        {
            if (currentState == value) return;
            currentState = value;
            updateMaterials();
            if (currentPlayer == null) return;
            refreshEffect();
            RegionOutline outline = GetComponent<RegionOutline>();
            if (State == RegionState.C)
                outline.EnhancePulse(outline.initialGrowRate * 3, outline.initialGrowFactor * 2);
        }
    }

    // Use this for initialization
    protected void Start () {
        State = initialState;
        refresh();
        ready = true;
	}

    private float DamageRate(float time) {
        return (200 / Mathf.Pow(volatileDurationUntilDeath, 2) - 2 * initialDmgRate / volatileDurationUntilDeath) * time + initialDmgRate;
    }

    private float DamageOverInterval(float time, float prevTime) {
        return time * DamageRate(time) - prevTime * DamageRate(prevTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPlayer != null && currentEffect == RegionEffect.Volatile)
        {
            damagePlayer();
        }
    }

    /// <summary>
    /// Is *only* called immediately after one of two events occurs:
    ///   1) The region changes state while occupied by a player
    ///   2) A player enters the region area
    /// </summary>
    private void refreshEffect() {
        currentEffect = StateToEffect(State, currentPlayer.GetComponent<Player>().playerID);
        if (currentEffect == RegionEffect.Unstable)
            currentPlayer.GetComponent<Player>().Kill();
        else if (currentEffect == RegionEffect.Volatile)
        {
            prevTime = 0f;
            volatileTimer = 0f;
        }

        // Region is stable
        if (IsGoal)
        {
            GameObject.FindWithTag("LevelController").GetComponent<LevelController>().ProgressLevel(50);
        }
    }

    public void SetOccupied(bool isOccupied, Transform player)
    {
        RegionOutline outline = GetComponent<RegionOutline>();
        if (isOccupied && player != null) {
            if (player == currentPlayer)
            {
                Debug.LogError(string.Format("REGION[{0}] Occupied:", name) + string.Format("Player {0} is attempting to occupy {1} twice", player.name, name));
                return;
            }
            else if (currentPlayer != null)
            {
                currentPlayer.GetComponent<Player>().Kill();
                player.GetComponent<Player>().Kill();
                currentPlayer = null;
                // SetOccupied(false, currentPlayer); // Uncomment this if bug occurs where region remains active after players die?
                return;
            }
            currentPlayer = player;
            refreshEffect();
            if (State == RegionState.C)
                outline.EnhancePulse(outline.initialGrowRate * 3, outline.initialGrowFactor * 2);
            outline.IsActive = true;
        }
        else if (!isOccupied)
        {
            if (currentPlayer == null)
                Debug.LogError(string.Format("REGION[{0}] Leaving:", name) + "Cannot de-occupy a region that is not occupied " + string.Format("({0}, {1})", player.name, name));
            currentPlayer = null;
            outline.IsActive = false;
            outline.ResetPulse();

            // Update level completion percent if player leaves goal
            if (IsGoal)
            {
                GameObject.FindWithTag("LevelController").GetComponent<LevelController>().ProgressLevel(-50);
            }
        }
    }

    	protected void refresh() {
        refreshColliders();
        updateMaterials();
        GetComponent<RegionOutline>().Refresh();
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

    private void damagePlayer()
    {
        volatileTimer += Time.deltaTime;
        CurrentPlayer.GetComponent<Player>().TakeDamage(DamageOverInterval(volatileTimer, prevTime));
        prevTime = volatileTimer;
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

    public void ExecuteStateChange(ActionType action, PlayerID player, bool isSource = true)
    {
        if (IsGoal) return; // Goal regions should never change.

        if(!isSource || ActionDictionary.AffectsSourceRegion(action))
        {
            State = ActionDictionary.GetActionEffect(action, State, player);
        }

        if (!isSource) return;

        IEnumerable<Region> neighbours = Neighbours.Where(n => n != null).Select(neighbour => neighbour.GetComponent<Region>());
        foreach (Region neighbour in neighbours)
        {
            if (neighbour == null)
            {
                Debug.Log("Neighbour doesn't have Region component?");
                continue;
            }
            neighbour.ExecuteStateChange(action, player, false);
        }
    }

    public static RegionEffect StateToEffect(RegionState state, PlayerID player)
    {
        if (state == RegionState.C)
            return RegionEffect.Volatile;
        else if ((player == PlayerID.P1 && state == RegionState.A) || (player == PlayerID.P2 && state == RegionState.B))
            return RegionEffect.Stable;
        else
            return RegionEffect.Unstable;
    }

    public IEnumerator GetEnumerator()
    {
        return hexTiles.GetEnumerator();
    }
}

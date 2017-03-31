using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour {

    public List<GameObject> regions;
    public GameObject player1prefab;
    public GameObject player2prefab;

    public float redRegionHeightOffset = 0.5f;

    /// <summary>
    /// Materials for the tile meshes. Each element in this arrays corresponds to one of the three region states (A, B, C).
    /// </summary>
    [Header("Materials")]
    public Material[] TileMaterials = new Material[3];
    /// <summary>
    /// Materials for the tile outlines. Each element in this arrays corresponds to one of the three region states (A, B, C).
    /// </summary>
    public Material[] OutlineMaterials = new Material[3];

    [Header("Tile generator settings")]
    public GameObject hexGeneratorPrefab;
    public BoardShape shape = BoardShape.Rectangle;
    public int width = 4, length = 6;

    public int p1StartingRegion = -1;
    public int p2StartingRegion = -1;

    public int p1GoalRegion = -1;
    public int p2GoalRegion = -1;
    public GameObject PlayerGoal;

    [Header("Time Pressure")]
    public bool TimePressureEnabled = false;
    public float timePressureDelay = 20f; // time before time pressure is first initiated (interval countdown starts)
    public float timePressureInterval = 10f; // intervals between time pressure increases

    /// <summary>True iff the board has zero regions</summary>
    public bool IsEmpty { get { return regions.Count == 0; } }

    // Tile generator
    private GameObject generatorObj;
    private float pressureCookerTimer = 0f;
    private int pressureRow = 0;
    private IEnumerable<HexGridCoordinates> tileLocationsByRow;

    private float startTime;

    private void Start()
    {
        FixRegionList();
        if (GameObject.FindGameObjectWithTag("Player1") != null || GameObject.FindGameObjectWithTag("Player2") != null)
            Debug.LogError("Players are already placed in scene.  Remove players from the scene -- they will be spawned by the game board.");
        if (generatorObj == null)
            generatorObj = GetComponentInChildren<HexGridGenerator>().gameObject;
        if (TimePressureEnabled)
            initializeTimePressure();
        if (p1StartingRegion >= 0 && p2StartingRegion >= 0)
            StartCoroutine(SpawnPlayers());
        if (p1GoalRegion >= 0 && p2GoalRegion >= 0)
            StartCoroutine(SpawnGoals());
        startTime = Time.time;
    }

    private GameObject instantiatePlayer(PlayerID p, Transform parent)
    {
        if (p == PlayerID.Both) throw new ArgumentException("Player cannot be 'both'");
        GameObject prefab = (p == PlayerID.P1) ? player1prefab : player2prefab;
        GameObject playerObj = Instantiate(prefab, parent);
        playerObj.name = p.ToString();
        return playerObj;
    }

    public IEnumerator SpawnPlayers()
    {
        yield return new WaitUntil(() => regions.All(r => r.GetComponent<Region>().IsReady));
        GameObject players = new GameObject("Players");
        players.transform.position = transform.position;
        Region p1Start, p2Start;

        try
        {
            p1Start = regions
                .Where(obj => obj != null)
                .Where(obj => obj.name == "Region " + p1StartingRegion.ToString())
                .First()
                .GetComponent<Region>();
            p2Start = regions
                .Where(obj => obj != null)
                .Where(obj => obj.name == "Region " + p2StartingRegion.ToString())
                .First()
                .GetComponent<Region>();
        }
        catch (Exception e)
        {
            Debug.LogError("Could not find players' starting regions.  Ensure that starting region numbers are set, and that those regions exist.  The number should be in the name of the Region's GameObject (i.e. 'Region X').");
            throw e;
        }

        Player p1 = instantiatePlayer(PlayerID.P1, players.transform).GetComponent<Player>();
        Player p2 = instantiatePlayer(PlayerID.P2, players.transform).GetComponent<Player>();

        CameraMover cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMover>();
        if (cam != null)
        {
            cam.SetPlayers(p1.transform, p2.transform);
        }

        p1.startingRegion = p1Start.gameObject;
        p2.startingRegion = p2Start.gameObject;
        p1.Spawn();
        p2.Spawn();
    }

    public IEnumerator SpawnGoals()
    {
        yield return new WaitUntil(() => regions.All(r => r.GetComponent<Region>().IsReady));
        GameObject goals = new GameObject("Goals");
        goals.transform.position = transform.position;

        Region p1Goal = regions
            .Where(obj => obj != null)
            .First(obj => obj.name == "Region " + p1GoalRegion.ToString())
            .GetComponent<Region>();
        Region p2Goal = regions
            .Where(obj => obj != null)
            .First(obj => obj.name == "Region " + p2GoalRegion.ToString())
            .GetComponent<Region>();

        GameObject p1GoalObject = instantiateGoal(p1Goal, goals.transform, "P1Goal");

        GameObject p2GoalObject = instantiateGoal(p2Goal, goals.transform, "P2Goal");
    }

    private GameObject instantiateGoal(Region goalRegion, Transform parent, String name)
    {
        GameObject goalObject = Instantiate(PlayerGoal, goalRegion.transform.GetChild(0).transform.position, Quaternion.identity, parent);
        goalObject.transform.GetChild(0).GetComponent<MeshRenderer>().material =
            TileMaterials[(int) goalRegion.State];
        goalObject.transform.SetParent(parent);
        goalObject.name = name;

        goalRegion.IsGoal = true;

        return goalObject;
    }

    private void Update()
    {
        if (TimePressureEnabled) ApplyTimePressure();
    }

    private void ApplyTimePressure()
    {
        if (Time.timeSinceLevelLoad >= timePressureDelay || Time.time - startTime > timePressureDelay)
        {
            // Initial condition for starting time pressure
            pressureCookerTimer += Time.deltaTime;
            Debug.Log("increasing timer");
        }

        if (pressureCookerTimer >= timePressureInterval)
        {
            HexGridGenerator generator = generatorObj.GetComponentInChildren<HexGridGenerator>();
            if (generator == null) { Debug.LogError("Missing tile mom!"); pressureCookerTimer = 0f; return; }
            Debug.Log("Applying time pressure change.");
            IEnumerable<Region> firstRowRegions = tileLocationsByRow
                .TakeWhile(loc => loc.row <= pressureRow)
                .Where(loc => generator.ContainsTileAt(loc))
                .Select(loc => generator[loc])
                .Where(obj => obj != null)
                .Select(obj => obj.GetComponent<HexMesh>().GetComponentInParent<Region>())
                .Where(r => r != null)
                .Distinct();
            foreach (Region r in firstRowRegions)
                r.State = RegionState.C;
            pressureRow++;
            pressureCookerTimer = 0f;
        }
    }

    private void initializeTimePressure()
    {
        Debug.Log("Initializing time pressure.");
        HexGridGenerator generator;
        if (generatorObj != null) generator = generatorObj.GetComponent<HexGridGenerator>();
        else
        {
            generator = instantiateGenerator();
            generator.recoverLostTileGrid();
        }
        tileLocationsByRow = generator.TileLocations.OrderBy(loc => loc.row);
        if (tileLocationsByRow.Count() > 0)
            pressureRow = tileLocationsByRow.Min(loc => loc.row);
    }

    private void init() {
        if (regions == null) regions = new List<GameObject>();
    }

    /// <summary>
    /// Create the initial hex grid and group hex tiles into regions according to the board configuration
    /// </summary>
    [ContextMenu("Generate hex grid")]
    public void GenerateHexes() {
        init();
        HexGridGenerator generator = instantiateGenerator();
        generator.width = width;
        generator.length = length;
        generator.shape = shape;
        // Generate the Hex Grid
        generator.Generate();
    }

    private HexGridGenerator instantiateGenerator()
    {
        Debug.Log("Instantiating generator");
        if (generatorObj != null)
            if (Application.isPlaying) Destroy(generatorObj);
            else DestroyImmediate(generatorObj);
        // Instantiate the tile generator
        generatorObj = Instantiate(hexGeneratorPrefab, transform.position, Quaternion.identity, transform);
        generatorObj.name = "HexTileMom";
        return generatorObj.GetComponent<HexGridGenerator>();
    }

    [ContextMenu("Generate outer wall")]
    private void generateWalls()
    {
        GetComponentInChildren<HexGridGenerator>().GenerateWalls();
    }

    [ContextMenu("Destroy outer wall")]
    private void destroyWalls()
    {
        GetComponentInChildren<HexGridGenerator>().DestroyWalls();
    }

    [ContextMenu("Reset region outlines")]
    private void resetRegionOutlines() {
        foreach (RegionOutline outline in regions.Where(r => r != null).Select(obj => obj.GetComponent<RegionOutline>()).Where(r => r != null)) {
            outline.Refresh();
        }
    }

    /// <summary>
    /// Instantiate the regions of the board and use any remaining tiles in the generator to create one-tile regions
    /// </summary>
    [ContextMenu("Consolidate all regions")]
    public void ConsolidateRegions() {
        if (generatorObj != null)
            while (generatorObj.transform.childCount != 0)
            {
                CreateRegionWithTiles(new Transform[] { generatorObj.transform.GetChild(0) });
            }
        foreach (RegionBuilder r in regions.Where(obj => obj != null).Select(obj => obj.GetComponent<RegionBuilder>())) {
            if (r != null)
                r.Consolidate();
        }
    }

    private IEnumerable<Transform> transformIterator(Transform t) {
        foreach (Transform ch in t)
            yield return ch;
    }

    public RegionBuilder CreateRegionWithTiles(IEnumerable<Transform> tiles) {
        tiles = tiles.Where(t => t.GetComponent<HexMesh>() != null);
        if (tiles.Count() == 0) return null;
        RegionBuilder newRegion = createEmptyRegion();
        newRegion.hexTilesToAdd = tiles.Select(t => t.gameObject).ToArray();
        newRegion.Consolidate();
        return newRegion;
    }


    private RegionBuilder createEmptyRegion() {
        RegionBuilder region = new GameObject("Region " + (regions.Count + 1), typeof(RegionBuilder)).GetComponent<RegionBuilder>();
        region.transform.position = transform.position;
        region.transform.SetParent(transform);
        region.TileMaterials = (Material[])TileMaterials.Clone();
        region.OutlineMaterials = (Material[])OutlineMaterials.Clone();
        int layer = LayerMask.NameToLayer("Regions");
        if (layer > 0)
            region.gameObject.layer = LayerMask.NameToLayer("Regions");
        regions.Add(region.gameObject);
        return region;
    }

    [ContextMenu("Fix region list (if it has null values)")]
    public void FixRegionList() {
        regions.RemoveAll(r => r == null || r.GetComponent<Region>() == null);
    }
}

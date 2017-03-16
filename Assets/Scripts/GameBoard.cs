using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour {

    public List<GameObject> regions;

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

    [Header("Time Pressure")]
    public bool TimePressureEnabled = false;
    public float timePressureDelay = 20f; // time before time pressure is first initiated (interval countdown starts)
    public float timePressureInterval = 10f; // intervals between time pressure increases

    /// <summary>
    /// Denotes whether actions propragate a distance of 1 or 2 regions out from the source region.
    /// </summary>
    [Range(1, 2)] public int StateChangePropagationDistance = 1;

    /// <summary>True iff the board has zero regions</summary>
    public bool IsEmpty { get { return regions.Count == 0; } }

    // Tile generator
    private GameObject generatorObj;
    private float pressureCookerTimer = 0f;
    private int pressureRow = 0;
    private IEnumerable<HexGridCoordinates> tileLocationsByRow;

    private void Start()
    {
        if (p1StartingRegion >= 0 && p2StartingRegion >= 0)
            SpawnPlayers();
    }

    public void SpawnPlayers()
    {
        Region p1Start = regions.Where(obj => obj != null).Where(obj => obj.name == "Region " + p1StartingRegion.ToString()).First().GetComponent<Region>();
        Region p2Start = regions.Where(obj => obj != null).Where(obj => obj.name == "Region " + p2StartingRegion.ToString()).First().GetComponent<Region>();
        PlayerMovementController p1 = GameObject.FindGameObjectWithTag("Player1").GetComponent<PlayerMovementController>();
        p1.startingRegion = p1Start.gameObject;
        PlayerMovementController p2 = GameObject.FindGameObjectWithTag("Player2").GetComponent<PlayerMovementController>();
        p2.startingRegion = p2Start.gameObject;
        p1.Spawn();
        p2.Spawn();
    }

    private void Update()
    {
        if (TimePressureEnabled) ApplyTimePressure();
    }

    private void ApplyTimePressure()
    {
        if (timePressureDelay < Time.timeSinceLevelLoad && timePressureDelay + timePressureInterval < Time.timeSinceLevelLoad)
            // Initial condition for starting time pressure
            initializeTimePressure();

        if (pressureCookerTimer >= timePressureInterval)
        {
            HexGridGenerator generator = generatorObj.GetComponent<HexGridGenerator>();
            IEnumerable<Region> firstRowRegions = tileLocationsByRow
                .TakeWhile(loc => loc.row <= pressureRow)
                .Select(loc => generator[loc])
                .Where(obj => obj != null)
                .Select(obj => obj.GetComponent<HexMesh>().GetComponentInParent<Region>())
                .Distinct();
            foreach (Region r in firstRowRegions)
                r.State = RegionState.C;
            pressureRow++;
            pressureCookerTimer = 0f;
        }
        pressureCookerTimer += Time.deltaTime;
    }

    private void initializeTimePressure()
    {
        pressureCookerTimer += Time.deltaTime;
        HexGridGenerator generator;
        if (generatorObj != null) generator = generatorObj.GetComponent<HexGridGenerator>();
        else if ((generator = GetComponentInChildren<HexGridGenerator>()) == null)
        {
            generator = instantiateGenerator();
            generator.recoverLostTileGrid();
        }
        tileLocationsByRow = generator.TileLocations.OrderBy(loc => loc.row);
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
}

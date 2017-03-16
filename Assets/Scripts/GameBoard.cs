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

    public int p1StartingRegion;
    public int p2StartingRegion;

    public bool TimePressureEnabled = false;

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

    private void Start()
    {
        Region p1Start = regions.Where(obj => obj != null).Where(obj => obj.name == "Region " + p1StartingRegion.ToString()).First().GetComponent<Region>();
        Region p2Start = regions.Where(obj => obj != null).Where(obj => obj.name == "Region " + p2StartingRegion.ToString()).First().GetComponent<Region>();
        GameObject.FindGameObjectWithTag("Player1").GetComponent<PlayerMovementController>().startingRegion = p1Start.gameObject;
        GameObject.FindGameObjectWithTag("Player2").GetComponent<PlayerMovementController>().startingRegion = p2Start.gameObject;
    }

    private void Update()
    {
        if (TimePressureEnabled) ApplyTimePressure();
    }

    private void ApplyTimePressure()
    {
        if (Time.timeSinceLevelLoad >= 30)
        {
            pressureCookerTimer += Time.deltaTime;

        }
        if (pressureCookerTimer >= 10) ;
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
        if (generatorObj != null)
            if (Application.isPlaying) Destroy(generatorObj);
            else DestroyImmediate(generatorObj);
        // Instantiate the tile generator
        generatorObj = Instantiate(hexGeneratorPrefab, transform.position, Quaternion.identity, transform);
        generatorObj.name = "HexTileMom";
        HexGridGenerator generator = generatorObj.GetComponent<HexGridGenerator>();
        generator.width = width;
        generator.length = length;
        generator.shape = shape;
        // Generate the Hex Grid
        generator.Generate();
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

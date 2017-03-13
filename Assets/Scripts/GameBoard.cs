using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour {

    public List<GameObject> regions;
    [Header("Materials")]
    public Material[] TileMaterials = new Material[3];
    public Material[] OutlineMaterials = new Material[3];
    [Header("Tile generator settings")]
    public GameObject hexGeneratorPrefab;
    public BoardShape shape = BoardShape.Rectangle;
    public int width = 4, length = 6;

    public bool IsEmpty { get { return regions.Count == 0; } }

    private bool isGenerated = false;
    private GameObject genObj;

    private void init() {
        if (regions == null) regions = new List<GameObject>();
    }

    /// <summary>
    /// Create the initial hex grid and group hex tiles into regions according to the board configuration
    /// </summary>
    [ContextMenu("Generate hex grid")]
    public void GenerateHexes() {
        init();
        if (genObj != null)
            if (Application.isPlaying) Destroy(genObj);
            else DestroyImmediate(genObj);
        // Instantiate the tile generator
        genObj = Instantiate(hexGeneratorPrefab, transform.position, Quaternion.identity, transform);
        genObj.name = "HexTileMom";
        HexGridGenerator generator = genObj.GetComponent<HexGridGenerator>();
        generator.width = width;
        generator.length = length;
        generator.shape = shape;
        // Generate the Hex Grid
        generator.Generate();
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
        if (genObj != null)
            while (genObj.transform.childCount != 0)
            {
                CreateRegionWithTiles(new Transform[] { genObj.transform.GetChild(0) });
            }
        foreach (Region r in regions.Where(obj => obj != null).Select(obj => obj.GetComponent<Region>())) {
            if (r != null)
                r.Consolidate();
        }
    }

    private IEnumerable<Transform> transformIterator(Transform t) {
        foreach (Transform ch in t)
            yield return ch;
    }

    public Region CreateRegionWithTiles(IEnumerable<Transform> tiles) {
        tiles = tiles.Where(t => t.GetComponent<HexMesh>() != null);
        if (tiles.Count() == 0) return null;
        Region newRegion = createEmptyRegion();
        newRegion.hexTilesToAdd = tiles.Select(t => t.gameObject).ToArray();
        newRegion.Consolidate();
        return newRegion;
    }


    private Region createEmptyRegion() {
        Region region = new GameObject("Region " + (regions.Count + 1), typeof(Region)).GetComponent<Region>();
        region.transform.position = transform.position;
        region.transform.SetParent(transform);
        region.TileMaterials = (Material[])TileMaterials.Clone();
        region.OutlineMaterials = (Material[])OutlineMaterials.Clone();
        regions.Add(region.gameObject);
        return region;
    }
}

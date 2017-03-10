using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RegionGroup
{
    public Transform root;
    public List<Transform> children;
}

public class GameBoard : MonoBehaviour {
    
    /// Must have components: HexGridGenerator, BoardConfiguration
    public GameObject hexTileGenerator;
    public GameObject regionPrefab;

    private GameObject generator;

    public Material tileA;
    public Material tileB;
    public Material tileC;
    public Material outlineA;
    public Material outlineB;
    public Material outlineC;

    /// <summary>
    /// Generate the board objects
    /// </summary>
    [ContextMenu("Generate Board")]
    public void Generate() {
        generateHexes();
        ConsolidateRegions();
        cleanUp();
    }

    /// <summary>
    /// Create the initial hex grid and group hex tiles into regions according to the board configuration
    /// </summary>
    private void generateHexes() {
        // Instantiate the tile generator
        generator = Instantiate(hexTileGenerator, transform, true);
        // Generate the Hex Grid
        generator.GetComponent<HexGridGenerator>().Generate();
        // Consolidate the hexes into region groups according to the board configuration
        generator.GetComponent<BoardConfiguration>().Consolidate();
    }

    /// <summary>
    /// Instantiate the regions of the board and use the hex grid groupings to create region meshes
    /// </summary>
    void ConsolidateRegions() {
        List<RegionGroup> transforms = new List<RegionGroup>();
        foreach (Transform child in generator.transform)
        {
            RegionGroup r;
            r.root = child;
            r.children = new List<Transform>();
            if (child.childCount > 0)
                foreach (Transform subChild in child)
                    r.children.Add(subChild);
            transforms.Add(r);
        }
        foreach (RegionGroup r in transforms)
        {
            Region region = Instantiate(regionPrefab, transform, true).GetComponent<Region>();
            region.transform.SetParent(transform);
            foreach (Transform ch in r.children) ch.SetParent(region.transform);
            r.root.SetParent(region.transform);
            region.MakeRegionFromChildren();

            region.TileMaterials = new Material[] { tileA, tileB, tileC };
            region.OutlineMaterials = new Material[] { outlineA, outlineB, outlineC };
            region.ShiftState(Mathf.RoundToInt(Random.Range(0, 3)));

            if (Application.isPlaying) Destroy(r.root.gameObject);
            else DestroyImmediate(r.root.gameObject);

            foreach (Transform ch in r.children)
                if (Application.isPlaying) Destroy(ch.gameObject);
                else DestroyImmediate(ch.gameObject);
        }
    }

    /// <summary>
    /// Destroy the initial hex grid after regions are created
    /// </summary>
    private void cleanUp()
    {
        generator.GetComponent<HexGridGenerator>().DestroyAll();
        if (Application.isPlaying) Destroy(generator.gameObject);
        else DestroyImmediate(generator.gameObject);
    }
}

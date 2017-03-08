using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RegionGroup
{
    public Transform root;
    public List<Transform> children;
}

public class GameBoard : MonoBehaviour {

    public GameObject hexTileGenerator;
    public GameObject regionTemplate;

    private GameObject generator;

    [ContextMenu("Generate Board")]
    public void Generate() {
        generateHexes();
        ConsolidateRegions();
        cleanUp();

    }

    private void generateHexes() {
        generator = Instantiate(hexTileGenerator, transform, true);
        generator.GetComponent<HexGridGenerator>().Generate();
        generator.GetComponent<BoardConfiguration>().Consolidate();
    }

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
            Region region = Instantiate(regionTemplate, transform, true).GetComponent<Region>();
            region.transform.SetParent(transform);
            foreach (Transform ch in r.children) ch.SetParent(region.transform);
            r.root.SetParent(region.transform);
            region.FlattenChildMeshes();

            if (Application.isPlaying) Destroy(r.root.gameObject);
            else DestroyImmediate(r.root.gameObject);

            foreach (Transform ch in r.children)
                if (Application.isPlaying) Destroy(ch.gameObject);
                else DestroyImmediate(ch.gameObject);
        }
    }

    private void cleanUp()
    {
        generator.GetComponent<HexGridGenerator>().DestroyAll();
        if (Application.isPlaying) Destroy(generator.gameObject);
        else DestroyImmediate(generator.gameObject);
    }
}

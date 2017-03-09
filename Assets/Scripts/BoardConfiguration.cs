using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardConfiguration : MonoBehaviour {

    public string boardLayout;

    private Dictionary<string, List<int[]>> layoutMap = new Dictionary<string, List<int[]>> {
        { "TestConfig1", TestConfig1.regions }
    };

    private List<int[]> regions;

    [ContextMenu("Consolidate")]
    public void Consolidate()
    {
        regions = layoutMap[boardLayout];
        List<RegionGroup> groups = new List<RegionGroup>();
        foreach (int[] regionSpec in regions)
        {
            RegionGroup group;
            group.root = transform.GetChild(regionSpec[0]);
            group.children = new List<Transform>();
            for (int i = 1; i < regionSpec.Length; i++)
                group.children.Add(transform.GetChild(regionSpec[i]));
            groups.Add(group);
        }
        foreach (RegionGroup g in groups)
            foreach (Transform child in g.children)
                child.SetParent(g.root);
    }
}

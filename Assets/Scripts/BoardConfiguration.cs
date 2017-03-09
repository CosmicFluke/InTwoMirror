using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct HexTileLocation {
    public enum OffsetType { Left, Right, Centre }
    public readonly int row;
    public readonly int offset;
    public readonly bool evenRowSize;
    public readonly OffsetType offsetType;
    public HexTileLocation(int row, int offset, bool evenRowSize, OffsetType offsetType = OffsetType.Centre) {
        if (evenRowSize && offsetType == OffsetType.Centre && offset == 0)
            throw new Exception("Offset from centre cannot be zero in an even-width row.");
        this.row = row;
        this.offset = offset;
        this.evenRowSize = evenRowSize;
        this.offsetType = offsetType;
    }
}

public struct RegionSpec {
    public int id;
    public HexTileLocation[] hexes;
    public int[][] joinedEdges;
    public RegionSpec(int id, List<HexTileLocation> hexes, List<int[]> joinedEdges) {
        this.id = id;
        this.hexes = hexes.ToArray();
        this.joinedEdges = joinedEdges.ToArray();
    }
}

public struct BoardSpec {
    public IEnumerable<HexTileLocation[]> Regions { get { return regions.AsEnumerable(); } }

    public BoardShape shape;
    public int width, length;
    public List<HexTileLocation[]> regions;

    public HexTileLocation DefineHex(int row, int offset)
    {
        return new HexTileLocation(row, offset, (width % 2) != (row % 2));
    }
}

public enum BoardShape { Rectangle, Diamond };

public class BoardConfiguration : MonoBehaviour {

    public string boardLayoutString;

    private BoardSpec boardSpec;

    [ContextMenu("Consolidate")]
    public void Consolidate()
    {
        boardSpec = BoardLayouts.layoutMap[boardLayoutString];
        List<RegionGroup> groups = new List<RegionGroup>();
        foreach (HexTileLocation[] regionSpec in boardSpec.Regions)
        {
            RegionGroup group;
            HexGridGenerator gen = GetComponent<HexGridGenerator>();
            HexTileLocation root = regionSpec[0];
            group.root = gen[root.row, gen.OffsetToIndexInRow(root.offset)].transform;
            group.children = new List<Transform>();
            for (int i = 1; i < regionSpec.Length; i++)
            {
                HexTileLocation loc = regionSpec[i];
                group.children.Add(gen[loc.row, gen.OffsetToIndexInRow(loc.offset)].transform);
            }
            groups.Add(group);
        }
        foreach (RegionGroup g in groups)
            foreach (Transform child in g.children)
                child.SetParent(g.root);
    }
}

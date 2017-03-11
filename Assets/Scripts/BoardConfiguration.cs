using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct HexTileLocation {
    public enum OffsetType { Left, Right, Centre }
    public int row;
    public int offset;
    public bool evenRowSize;
    public OffsetType offsetType;
    public HexTileLocation(int row, int offset, bool evenRowSize, OffsetType offsetType = OffsetType.Centre) {
        if (evenRowSize && offsetType == OffsetType.Centre && offset == 0)
            throw new Exception("Offset from centre cannot be zero in an even-width row.");
        this.row = row;
        this.offset = offset;
        this.evenRowSize = evenRowSize;
        this.offsetType = offsetType;
    }
    public override string ToString() {
        return string.Format("HexTileLocation(row: {0}, offset: {1} from {2}, evenRowWidth: {3})", row, offset, offsetType, evenRowSize);
    }
}

public struct RegionSpecification {
    public int id;
    public HexTileLocation[] hexes;
    public RegionState initialState;
    public RegionSpecification(int id, List<HexTileLocation> hexes, RegionState initialState = RegionState.A) {
        this.id = id;
        this.hexes = hexes.ToArray();
        this.initialState = initialState;
    }
}

public struct BoardLayout {
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

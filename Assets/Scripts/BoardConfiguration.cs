using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct HexGridCoordinates {
    public enum OffsetType { Left, Right, Centre }
    public int row;
    public int offset;
    public bool evenRowSize;
    public OffsetType offsetType;
    public HexGridCoordinates(int row, int offset, bool evenRowSize, OffsetType offsetType = OffsetType.Centre) {
        if (evenRowSize && offsetType == OffsetType.Centre && offset == 0)
            throw new Exception("Offset from centre cannot be zero in an even-width row.");
        this.row = row;
        this.offset = offset;
        this.evenRowSize = evenRowSize;
        this.offsetType = offsetType;
    }

    public override string ToString()
    {
        return string.Format("HexTileLocation(row: {0}, offset: {1} from {2}, evenRowWidth: {3})", row, offset, offsetType, evenRowSize);
    }

    public override bool Equals(object obj)
    {
        return obj != null && obj is HexGridCoordinates && Equals((HexGridCoordinates)obj);
    }

    public bool Equals(HexGridCoordinates other)
    {
        return row == other.row && offset == other.offset && evenRowSize == other.evenRowSize && offsetType == other.offsetType;
    }

    public override int GetHashCode()
    {
        return (53 * (row << 11)) ^ (199 * (offset << 5)) ^ (23 * (evenRowSize ? 5 : 2) << 3) ^ (11 * (int) offsetType);
    }

    public static bool operator ==(HexGridCoordinates a, HexGridCoordinates b) { return a.Equals(b); }

    public static bool operator !=(HexGridCoordinates a, HexGridCoordinates b) { return !(a == b); }
}

public struct RegionSpecification {
    public int id;
    public HexGridCoordinates[] hexes;
    public RegionState initialState;
    public RegionSpecification(int id, List<HexGridCoordinates> hexes, RegionState initialState = RegionState.A) {
        this.id = id;
        this.hexes = hexes.ToArray();
        this.initialState = initialState;
    }
}

public struct BoardLayout {
    public IEnumerable<HexGridCoordinates[]> Regions { get { return regions.AsEnumerable(); } }

    public BoardShape shape;
    public int width, length;
    public List<HexGridCoordinates[]> regions;

    public HexGridCoordinates DefineHex(int row, int offset)
    {
        return new HexGridCoordinates(row, offset, (width % 2) != (row % 2));
    }
}

public enum BoardShape { Rectangle, Diamond };

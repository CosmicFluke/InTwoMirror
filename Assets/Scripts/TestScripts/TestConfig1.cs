using System.Collections.Generic;
using UnityEngine;

public class TestConfig1 {

    public BoardLayout Board { get { return board; } }
    private BoardLayout board;

    public TestConfig1() { 
        board.shape = BoardShape.Rectangle;
        board.width = 4;
        board.length = 6;

        List<HexTileLocation[]> regions = new List<HexTileLocation[]>();
        board.regions = regions;

        regions.Add(new HexTileLocation[] { board.DefineHex(1, -1), board.DefineHex(1, -2), board.DefineHex(2, -1) });
        regions.Add(new HexTileLocation[] { board.DefineHex(1, 1), board.DefineHex(2, 1) });
        regions.Add(new HexTileLocation[] { board.DefineHex(3, -2), board.DefineHex(3, -1), board.DefineHex(3, 1) });
        regions.Add(new HexTileLocation[] { board.DefineHex(5, -2), board.DefineHex(5, -1) });
        regions.Add(new HexTileLocation[] { board.DefineHex(4, 0), board.DefineHex(5, 1) });

        //RegionState[] regionStates =
    }
}

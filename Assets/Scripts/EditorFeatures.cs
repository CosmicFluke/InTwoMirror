using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EditorFeatures : ScriptableObject {

    [UnityEditor.MenuItem("HexTiles/Make region from selected tiles")]
    private static void makeRegionFromSelectedTiles()
    {
        IEnumerable<Transform> hexes = UnityEditor.Selection.GetTransforms(UnityEditor.SelectionMode.Editable)
            .Where(t => t.gameObject != null)
            .Where(t => t.GetComponent<HexMesh>() != null);
        GameBoard board = FindObjectOfType<GameBoard>();
        if (board == null) throw new System.Exception("Could not find a GameBoard.");
        board.CreateRegionWithTiles(hexes);
    }
}

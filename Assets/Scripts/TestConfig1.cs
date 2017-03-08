using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestConfig1 : ScriptableObject {

    public static List<int[]> regions = new List<int[]> {
        new int[] {3, 4, 8},
        new int[] {5, 9},
        new int[] {10, 11, 12},
        new int[] {17, 18},
        new int[] {15, 19}
    };
}

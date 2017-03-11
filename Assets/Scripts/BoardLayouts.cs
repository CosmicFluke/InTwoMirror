using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class BoardLayouts {
    public static Dictionary<string, BoardLayout> LayoutMap = new Dictionary<string, BoardLayout> {
        { "TestConfig1", new TestConfig1().Board }
    };
}

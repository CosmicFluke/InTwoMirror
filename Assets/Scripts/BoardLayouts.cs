using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class BoardLayouts {
    public static Dictionary<string, BoardSpec> layoutMap = new Dictionary<string, BoardSpec> {
        { "TestConfig1", new TestConfig1().Board }
    };
}

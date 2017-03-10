using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class BoardLayouts {
    public static Dictionary<string, Type> layoutMap = new Dictionary<string, Type> {
        { "TestConfig1", typeof(TestConfig1) }
    };
}

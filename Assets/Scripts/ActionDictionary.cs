using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDictionary {

    public static RegionState Lookup(Action action, RegionState fromState, PlayerID player) {
        switch (action)
        {
            case Action.Destabilize:
                // Does not affect own tile
                // Volatile -> Unstable
                // Stable -> Unstable
                // Unstable -> Unstable
                return (player == PlayerID.P1) ? RegionState.B : RegionState.A;
            //if (fromState == RegionState.C)
            //    return (player == PlayerID.P1) ? RegionState.A : RegionState.B;
            //else return (player == PlayerID.P1) ? RegionState.B : RegionState.A;
            case Action.Swap:
                // Affects own tile
                // Volatile -> Volatile
                // Stable -> Unstable
                // Unstable -> Stable
                if (fromState == RegionState.C) return fromState;
                else return (RegionState)((int)fromState ^ 1);
            default:
                return RegionState.C;
        }
    }
}

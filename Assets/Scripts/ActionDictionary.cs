using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDictionary {

    public static RegionState Lookup(Action action, RegionState fromState, PlayerID player) {
        switch (action)
        {
            case Action.Shift:
                return (RegionState)
                    (((int)fromState + 1) % System.Enum.GetNames(typeof(RegionState)).Length);
            case Action.Swap:
                if (fromState == RegionState.C) return fromState;
                return (RegionState)(Mathf.Abs((int)fromState - 1));
            case Action.Useless:
                if (fromState != RegionState.C) return RegionState.C;
                if (Region.StateToEffect(RegionState.A, player) == RegionEffect.Unstable) return RegionState.A;
                return RegionState.B;
            default:
                return RegionState.C;
        }
    }
}

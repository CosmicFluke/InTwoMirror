using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ActionEffectMap : SerializableDictionary<RegionState, RegionState> { }

public enum ActionType
{
    Swap,
    Destabilize
}

/// <summary>
/// [Pending refactoring]<para/>
/// Represents an action to be executed, and propagates through regions.
/// </summary>
public struct Action : System.IEquatable<Action> {

    private ActionEffectMap effect;
    private int distanceRemaining;
    private ActionType type;

    /// <summary>
    /// Construct a new action
    /// </summary>
    /// <param name="effects"></param>
    /// <param name="player"></param>
    public Action(ActionEffects effects, PlayerID player)
    {
        effect = (player == PlayerID.P1) ? effects.p1effects : (player == PlayerID.P2) ? effects.p2effects : ActionEffects.StaticEffect;
        distanceRemaining = effects.effectDistance;
        type = effects.type;
    }

    /// <summary>
    /// Use this *once* with the source region (player's current region), and then pass to the neighbouring regions .
    /// </summary>
    /// <param name="region"></param>
    public RegionState ApplyEffect(Region region) {
        // TODO: Implement!  Should change the region's state.
        distanceRemaining--;
        //region.ExecuteAction(this);
        return effect[region.State];
    }

    private RegionState GetActionEffect(RegionState state, RegionState effect)
    {
        return this.effect[state];
    }

    public bool Equals(Action obj) { return base.Equals(obj);  }
}

public struct ActionEffects : System.IEquatable<ActionEffects>
{
    // NOT IMPLEMENTED!!
    public static ActionEffectMap StaticEffect {
        get {
            throw new System.NotImplementedException();
            return (ActionEffectMap)(new SerializableDictionary<RegionState, RegionState> { });
        }
    }

    public ActionType type;
    public bool affectsCurrentRegion;
    public int effectDistance;
    public ActionEffectMap p1effects;
    public ActionEffectMap p2effects;

    public bool Equals(ActionEffects obj)
    {
        return base.Equals(obj);
    }
}

public class ActionDictionary : ScriptableSingleton<ActionDictionary>
{
    public static bool AffectsSourceRegion(ActionType action) {
        // TODO: re-implement using singleton instance, argument of type Action, and the ActionEffects
        // Will be redundant after refactor

        // TEMPORARY CODE
        switch (action)
        {
            case ActionType.Destabilize: return false;
            case ActionType.Swap: return true;
            default: return false;
        }
    }

    // Not yet implemented
    private Dictionary<ActionType, ActionEffects> actions = new Dictionary<ActionType, ActionEffects>();

    public static RegionState GetActionEffect(ActionType action, RegionState fromState, PlayerID player) {
        // TODO: Reimplement to access singleton instance, and using the ActionEffects dictionary

        switch (action)
        {
            case ActionType.Destabilize:
                // Does not affect own tile
                // Volatile -> Volatile
                // Stable -> Unstable
                // Unstable -> Unstable
                return (player == PlayerID.P1) ? RegionState.B : RegionState.A;
            //if (fromState == RegionState.C)
            //    return (player == PlayerID.P1) ? RegionState.A : RegionState.B;
            //else return (player == PlayerID.P1) ? RegionState.B : RegionState.A;
            case ActionType.Swap:
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

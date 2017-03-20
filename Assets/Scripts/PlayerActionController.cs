using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerActionController : MonoBehaviour
{
    private PlayerID player;

    [Range(0, 5)] public float actionDelay = 1f;

    private Region currentRegion;
    private float actionDelayCounter = 0f;

    // Use this for initialization
    void Start()
    {
        player = GetComponent<Player>().playerID;
    }

    void Update()
    {
        if (actionDelayCounter > actionDelay)
            actionDelayCounter = 0f;
        else if (actionDelayCounter > 0f)
            actionDelayCounter += Time.deltaTime;

        // NOTE: A = Stable, B = Unstable, C = Volatile
        if (Input.GetButtonDown(player.ToString() + "Action1") && actionDelayCounter == 0f)
        {
            ExecuteRegionAction(ActionType.Destabilize);
            actionDelayCounter += Time.deltaTime;
        }
        else if (Input.GetButtonDown(player.ToString() + "Action2") && actionDelayCounter == 0f)
        {
            ExecuteRegionAction(ActionType.Swap);
            actionDelayCounter += Time.deltaTime;
        }
    }

    /// <summary>
    /// Executes a region action where regions change from one state to another using the key and values in the action dictionary.
    /// </summary>
    /// <param name="action"></param>
    private void ExecuteRegionAction(ActionType action)
    {
        Debug.Log(player.ToString() + " executing " + action.ToString());
        currentRegion = GetComponent<Player>().Region;
        if (GetComponentInChildren<AnimatedCharacter>() == null)
            Debug.LogError("AnimatedCharacter component not found");
        GetComponentInChildren<AnimatedCharacter>().SetAnimation("Yell");
        GetComponent<SoundController>().startSound((int)action);

        currentRegion.ExecuteStateChange(action, player);
    }
}
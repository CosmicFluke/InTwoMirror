using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerActionController : MonoBehaviour
{
    public ActionType action0 = ActionType.Destabilize;
    public ActionType action1 = ActionType.None;
    public ActionType action2 = ActionType.None;

    [Range(0, 5)] public float actionDelay = 1f;

    private PlayerID player;
    private Region currentRegion;
    private ActionType[] actions;
    private string[] actionButtonNames;
    private float actionDelayCounter = 0f;

    // Use this for initialization
    void Start()
    {
        Player p = GetComponent<Player>();
        player = p.playerID;
        refreshActions();
        // Pre-assemble the action button name strings
        actionButtonNames = Enumerable.Range(0, 3)
            .Select(i => player.ToString() + "Action" + i.ToString())
            .ToArray();
    }

    void Update()
    {
        if (actionDelayCounter > actionDelay)
            actionDelayCounter = 0f;
        else if (actionDelayCounter > 0f)
            actionDelayCounter += Time.deltaTime;
        else
            checkForAction();
    }

    private void refreshActions()
    {
        actions = new ActionType[] { action0, action1, action2 };
    }

    private void checkForAction()
    {
        for (int i=0; i < 3; i++)
            if (actions[i] != ActionType.None && Input.GetButtonDown(actionButtonNames[i]))
            {
                ExecuteRegionAction(actions[i]);
                actionDelayCounter += Time.deltaTime;
                break;
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
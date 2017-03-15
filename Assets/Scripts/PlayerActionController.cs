using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
    public PlayerID player;

    public int[] actionInventory;

    public Region Region { get { return currentRegion; } }

    private Region currentRegion;
    private SoundController _soundController;
    private AnimatedCharacter characterAnimation;

    // Use this for initialization
    void Start()
    {
        if (player == PlayerID.Both) throw new System.Exception("Invalid player name for control script");

        _soundController = GetComponent<SoundController>();
        if (_soundController == null)
        {
            _soundController = gameObject.AddComponent<SoundController>();
        }

        characterAnimation = GetComponentInChildren<AnimatedCharacter>();
        if (characterAnimation == null)
            throw new System.Exception("This player object does not have a child with AnimatedCharacter.");
    }

    void Update()
    {
        // NOTE: A = Stable, B = Unstable, C = Volatile
        if (Input.GetButtonDown(player.ToString() + "Action1"))
        {
            ExecuteRegionAction(Action.Useless);
        }
        else if (Input.GetButtonDown(player.ToString() + "Action2"))
        {
           ExecuteRegionAction(Action.Swap);
        }
        else if (Input.GetButtonDown(player.ToString() + "Action3"))
        {
            ExecuteRegionAction(Action.Shift);
        }
    }

    /// <summary>
    /// Executes a region action where regions change from one state to another using the key and values in the action dictionary.
    /// </summary>
    /// <param name="action"></param>
    private void ExecuteRegionAction(Action action)
    {
        Debug.Log(player.ToString() + " executing " + action.ToString());
        currentRegion = GetComponent<PlayerMovementController>().Region;
        characterAnimation.SetAnimation("Yell");
        GetComponent<SoundController>().startSound((int)action);
        IEnumerable<Region> neighbours = currentRegion.Neighbours.Select(neighbour => neighbour.GetComponent<Region>());
        foreach (Region neighbour in neighbours)
        {
            if (neighbour == null) {
                Debug.Log("Neighbour doesn't have Region component?");
                continue;
            }
            neighbour.State = ActionDictionary.Lookup(action, neighbour.State, player);
            if (GameObject.FindWithTag("LevelController").GetComponent<LevelController>().actionPropagationDistance == 2)
            {
                foreach (Region neighbourNeighbour in neighbour.Neighbours.Select(neighbour2 => neighbour2.GetComponent<Region>()))
                {
                    if (!neighbours.Contains(neighbourNeighbour))
                    {
                        neighbour.State = ActionDictionary.Lookup(action, neighbour.State, player);
                    }
                }
            }
        }
    }
}
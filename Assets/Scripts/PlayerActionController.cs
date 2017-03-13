using System.Collections;
using System.Collections.Generic;
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
        if (characterAnimation == null) {
            throw new System.Exception("This player object does not have a child with AnimatedCharacter.");
        }


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
        characterAnimation.SetAnimation("Yell");
        currentRegion.PropagateAction(action, player, GameObject.FindWithTag("Board").GetComponent<GameBoard>().ActionDistance[(int) action] + 1);
        foreach (GameObject neighbour in currentRegion.Neighbours)
        {
            // TODO: Fix this, neighbours are just empty now, they should be an array of Regions, then we just do same as above.
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Region")
        {
            Region r = other.GetComponent<Region>();
            if (r != null)
                currentRegion = r;
        }
    }
}
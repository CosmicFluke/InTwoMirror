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

    void FixedUpdate()
    {
        // NOTE: A = Stable, B = Unstable, C = Volatile
        if (Input.GetButtonDown(player.ToString() + "Action1"))
        {
            Dictionary<RegionState, RegionState> ActionDictionary = new Dictionary<RegionState, RegionState>();
            ActionDictionary.Add(RegionState.B, RegionState.C);
            ActionDictionary.Add(RegionState.A, RegionState.C);
            ActionDictionary.Add(RegionState.C, RegionState.B);

            _soundController.startSound(1);
            ExecuteRegionAction(ActionDictionary);

            print(currentRegion.currentState);
        }
        else if (Input.GetButtonDown(player.ToString() + "Action2"))
        {
            Dictionary<RegionState, RegionState> ActionDictionary = new Dictionary<RegionState, RegionState>();
            ActionDictionary.Add(RegionState.A, RegionState.B);
            ActionDictionary.Add(RegionState.B, RegionState.A);
            ActionDictionary.Add(RegionState.C, RegionState.C);

            _soundController.startSound(2);
            ExecuteRegionAction(ActionDictionary);

            print(currentRegion.currentState);
        }
        else if (Input.GetButtonDown(player.ToString() + "Action3"))
        {
            Dictionary<RegionState, RegionState> ActionDictionary = new Dictionary<RegionState, RegionState>();
            ActionDictionary.Add(RegionState.A, RegionState.B);
            ActionDictionary.Add(RegionState.B, RegionState.C);
            ActionDictionary.Add(RegionState.C, RegionState.A);

            _soundController.startSound(3);
            ExecuteRegionAction(ActionDictionary);

            print(currentRegion.currentState);
        }
    }

    /// <summary>
    /// Executes a region action where regions change from one state to another using the key and values in the action dictionary.
    /// </summary>
    /// <param name="action"></param>
    private void ExecuteRegionAction(Dictionary<RegionState, RegionState> action)
    {
        characterAnimation.SetAnimation("Yell");
        currentRegion.State = action[currentRegion.currentState];
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
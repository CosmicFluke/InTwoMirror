using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
    public PlayerID player;

    Region CurrentRegion;

    public AudioClip ActionOneSound;
    public AudioClip ActionTwoSound;
    public AudioClip ActionThreeSound;

    private SoundController _soundController;

    public AnimatedCharacter Character;

    // Use this for initialization
    void Start()
    {
        if (player == PlayerID.Both) throw new System.Exception("Invalid player name for control script");

        _soundController = new SoundController(GetComponent<AudioSource>());
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

            _soundController.startSound(ActionOneSound);
            ExecuteRegionAction(ActionDictionary);

            print(CurrentRegion.currentState);
        }
        else if (Input.GetButtonDown(player.ToString() + "Action2"))
        {
            Dictionary<RegionState, RegionState> ActionDictionary = new Dictionary<RegionState, RegionState>();
            ActionDictionary.Add(RegionState.A, RegionState.B);
            ActionDictionary.Add(RegionState.B, RegionState.A);
            ActionDictionary.Add(RegionState.C, RegionState.C);

            _soundController.startSound(ActionTwoSound);
            ExecuteRegionAction(ActionDictionary);

            print(CurrentRegion.currentState);
        }
        else if (Input.GetButtonDown(player.ToString() + "Action3"))
        {
            Dictionary<RegionState, RegionState> ActionDictionary = new Dictionary<RegionState, RegionState>();
            ActionDictionary.Add(RegionState.A, RegionState.B);
            ActionDictionary.Add(RegionState.B, RegionState.C);
            ActionDictionary.Add(RegionState.C, RegionState.A);

            _soundController.startSound(ActionThreeSound);
            ExecuteRegionAction(ActionDictionary);

            print(CurrentRegion.currentState);
        }
    }

    /// <summary>
    /// Executes a region action where regions change from one state to another using the key and values in the action dictionary.
    /// </summary>
    /// <param name="action"></param>
    private void ExecuteRegionAction(Dictionary<RegionState, RegionState> action)
    {
        Character.SetAnimation("Yell");
        CurrentRegion.currentState = action[CurrentRegion.currentState];
        CurrentRegion.Consolidate(); // This should update the region's materials, right?
        foreach (GameObject neighbour in CurrentRegion.Neighbours)
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
                CurrentRegion = r;
        }
    }
}
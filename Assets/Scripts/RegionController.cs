using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RegionState { A, B, C }
public enum RegionEffect { Stable, Unstable, Volatile }
public enum Actions { Shift, Flip,  }

public class RegionController : MonoBehaviour {

    public static RegionEffect StateToEffect(RegionState state, int offset)
    {
        return (RegionEffect)(((int)state + offset) % 3);
    }

    public GameObject[] hexTiles;
    public GameObject[] neighbouringRegions;

    public RegionState State { get { return this.currentState; } }

    private RegionState currentState;

    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ShiftState(int offset) {
        currentState = (RegionState)(((int)currentState + offset) % 3);
    }
}

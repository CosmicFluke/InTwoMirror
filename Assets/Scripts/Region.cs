using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RegionState { A, B, C }
public enum RegionEffect { Stable, Unstable, Volatile }
public enum Actions { Shift, Flip, }

public class Region : MonoBehaviour {

    public RegionState State { get { return this.currentState; } }
    public RegionState currentState;
    private Material material;

    public static RegionEffect StateToEffect(RegionState state, PlayerID player)
    {
        if (state == RegionState.C) return RegionEffect.Volatile;
        else return player == PlayerID.P1 ? (RegionEffect)state : (RegionEffect)(((int)state - 1) * -1);
    }

    public void ShiftState(int offset)
    {
        currentState = (RegionState)(((int)currentState + offset) % 3);
        // Update hex color
        SetRegionColor();
    }

    public void SetRegionColor()
    {
        if (currentState == RegionState.A)
        {
            //material.color = Color.green;
            gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/tempTileA") as Material;
        }
        if (currentState == RegionState.B)
        {
            //material.color = Color.red;
            gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/tempTileB") as Material;
        }
        if (currentState == RegionState.C)
        {
            //material.color = Color.blue;
            gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/tempTileC") as Material;
        }
    }

    // Use this for initialization
    void Start () {
        SetRegionColor();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

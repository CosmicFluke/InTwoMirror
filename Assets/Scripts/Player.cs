﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlayerID
{
    P1,
    P2,
    Both
};

[RequireComponent(typeof(PlayerHealth), typeof(PlayerMovementController), typeof(PlayerActionController))]
public class Player : MonoBehaviour {

    public PlayerID playerID;
    public int startingHealthPoints = 100;
    [Range(0, 10)] public float RespawnDelay = 0.5f; // TODO: replace with death animation time?
    public bool invulnerable = false;

    public GameObject startingRegion;

    private Region currentRegion;
    private AnimatedCharacter characterAnimation;
    private bool dying = false;
    private bool isReady = false;

    private Vector3 spawnPoint;

    public Region Region { get { return currentRegion; } }

    // Use this for initialization
    void Start () {
        if (playerID == PlayerID.Both) throw new System.Exception("Invalid player name for control script");
        GetComponent<PlayerHealth>().InitializeHealth(startingHealthPoints);
        foreach (Transform child in transform)
            child.gameObject.layer = LayerMask.NameToLayer(playerID == PlayerID.P1 ? "Player1" : "Player2");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Spawn()
    {
        Region region = startingRegion.GetComponent<Region>();
        transform.position = region[0].transform.position + 2 * Vector3.up;
        spawnPoint = transform.position;
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        region.State = (playerID == PlayerID.P1) ? RegionState.A : RegionState.B;
        foreach (Region r in region.Neighbours.Select(n => n.GetComponent<Region>()).Where(r => r != null))
            r.State = r.initialState;
    }

    public void ChangeRegion(Transform other)
    {
        Region newRegion = other.GetComponent<Region>();
        if (currentRegion == newRegion) return;
        if (currentRegion != null)
            currentRegion.SetOccupied(false, transform);
        currentRegion = newRegion;
        // Debug.Log(playerID.ToString() + " changing region to " + currentRegion.gameObject.name);
        currentRegion.SetOccupied(true, transform);
    }

    public void TakeDamage(float amount) {
        if (dying || invulnerable) return;
        // Debug.Log(playerID + " takes " + amount + " damage");
        GetComponent<PlayerHealth>().ApplyDamage(amount);
    }

    public void Kill()
    {
        // Death lock
        if (!dying && !invulnerable) dying = true;
        else return;

        Debug.Log(name + " has died.");
        GetComponent<PlayerHealth>().HealthPoints = 0;
        AnimatedCharacter characterAnimation = GetComponentInChildren<AnimatedCharacter>();
        if (characterAnimation != null)
            characterAnimation.SetAnimation("Die");
        StartCoroutine(die());
    }

    IEnumerator die()
    {
        yield return new WaitForSeconds(RespawnDelay);
        Spawn();
        GetComponent<PlayerHealth>().HealthPoints = startingHealthPoints;
        dying = false; // Death unlock
    }

    public void SetActionType(int actionNumber, ActionType action)
    {
        PlayerActionController actionController = GetComponent<PlayerActionController>();
        switch (actionNumber)
        {
            case 0:
                actionController.action0 = action;
                break;
            case 1:
                actionController.action1 = action;
                break;
            case 2:
                actionController.action2 = action;
                break;
            default:
                throw new System.ArgumentException("Action number out of range: must be in {0, 1, 2}");
        }
    }

    //private void OnDrawGizmos()
    //{
    //    if (spawnPoint != null)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawSphere(spawnPoint, 0.25f);
    //    }
    //}
}

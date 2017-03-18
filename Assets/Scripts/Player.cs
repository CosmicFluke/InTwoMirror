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

    // TODO: Will likely be deprecated when GameBoard has full control of spawning.
    // When GameBoard does not have a starting region for the player, make this true and specify startingRegion
    public bool selfSpawn = true;
    // TODO: Deprecate this and specify starting region with GameBoard  (Let the board or the LevelController spawn the players)
    public GameObject startingRegion;

    private Region currentRegion;
    private AnimatedCharacter characterAnimation;
    private bool dying = false;

    public Region Region { get { return currentRegion; } }

    // Use this for initialization
    void Start () {
        if (playerID == PlayerID.Both) throw new System.Exception("Invalid player name for control script");
        GetComponent<PlayerHealth>().InitializeHealth(startingHealthPoints);
        if (selfSpawn) Spawn();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Spawn()
    {
        transform.position = startingRegion.GetComponent<Region>()[0].transform.position + 2 * Vector3.up;
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ChangeRegion(startingRegion.transform);
        currentRegion.State = (playerID == PlayerID.P1) ? RegionState.A : RegionState.B;
        foreach (Region r in currentRegion.Neighbours.Select(n => n.GetComponent<Region>()).Where(r => r != null))
            r.State = r.initialState;
    }

    public void ChangeRegion(Transform other)
    {
        if (currentRegion != null)
            currentRegion.SetOccupied(false, transform);
        currentRegion = other.GetComponent<Region>();
        Debug.Log(playerID.ToString() + " changing region to " + currentRegion.gameObject.name);
        currentRegion.SetOccupied(true, transform);
    }

    public void TakeDamage(float amount) {
        if (dying || invulnerable) return;
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
}

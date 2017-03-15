﻿﻿
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerID
{
    P1,
    P2,
    Both
};

public class PlayerMovementController : MonoBehaviour
{
    public float movementSpeed = 30f;
    // max distance player must be to interact with object
    public float maxActionDistance = 10f;
    public GameObject startingRegion;

    public PlayerID player;
    private GameObject otherPlayer;


    // Current game board region of the player
    private Region currentRegion;

    public Region Region
    {
        get { return currentRegion; }
    }

    // Temporary way to assign and access the two characters (??)
    public AnimatedCharacter characterAnimation;

    // Keeps track of volatile collision duration
    private float collisionStartTime;
    private float collisionCurrentDuration;
    private float collisionTotalDuration = 0f;

    private float deathCountdown = 10f;

    private PlayerHealth PlayerHealth;

    // Use this for initialization
    void Start()
    {
        if (player == PlayerID.Both) throw new System.Exception("Invalid player name for control script");
        currentRegion = startingRegion.GetComponent<Region>();

        // Assign the PlayerHealth object
        try
        {
            PlayerHealth = this.gameObject.GetComponent<PlayerHealth>();
        }
        catch
        {
            Debug.Log("No PlayerHealth object assigned to the GameObject!");
        }


        // identify other player
        otherPlayer = player == PlayerID.P1
            ? GameObject.Find("Player2")
            : player == PlayerID.P2 ? GameObject.Find("Player1") : null;

        characterAnimation = GetComponentInChildren<AnimatedCharacter>();
        if (characterAnimation == null)
            throw new System.Exception("This player object does not have a child with AnimatedCharacter.");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 movement = new Vector3(Input.GetAxis(player.ToString() + "Horizontal"), 0f,
            Input.GetAxis(player.ToString() + "Vertical"));
        Rigidbody rb = GetComponent<Rigidbody>();
        if (movement.magnitude > 0)
        {
            rb.velocity = movement * movementSpeed;

            // Rotate the character towards the direction of movement
            Quaternion newRotation = new Quaternion();
            newRotation.SetLookRotation(movement);
            transform.rotation = newRotation;

            // Call SetAnimation with parameter "Yell" to play the character's yelling animation
            characterAnimation.SetAnimation("Run");
        }
        else
        {
            characterAnimation.SetAnimation("Idle");
            rb.velocity = rb.velocity + (movement * movementSpeed);
        }
    }

    // Use Update to execute ongoing/gradual effects
    void Update()
    {
        ExecuteTileEffect();
        if (deathCountdown < 10f && deathCountdown > 0f)
            deathCountdown -= Time.deltaTime;
        if (deathCountdown <= 0) gameObject.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Region>() == currentRegion)
        {
            transform.GetComponent<Rigidbody>().velocity = -transform.GetComponent<Rigidbody>().velocity;
        }
    }

    // Use trigger callbacks to change the state of the character
    void OnTriggerEnter(Collider other)
    {
        changeRegion(other);
        collisionStartTime = Time.time;
    }

    private void changeRegion(Collider other)
    {
        Debug.Log(player.ToString() + " changing region to " + currentRegion.gameObject.name);
        currentRegion.SetOccupied(false, player);
        if (other.gameObject.layer == LayerMask.NameToLayer("Regions"))
        {
            Region r = other.GetComponent<Region>();
            currentRegion = r;
            r.SetOccupied(true, player);
        }
    }

    private void OnTriggerStay(Collider other)
    {
    }

    private void OnTriggerExit()
    {
        collisionTotalDuration = collisionCurrentDuration;
    }

    private void ExecuteTileEffect()
    {
        // Take damage from vola(tiles)^2
        switch (Region.StateToEffect(currentRegion.State, player))
        {
            case RegionEffect.Volatile:
                collisionCurrentDuration = collisionTotalDuration + Time.time - collisionStartTime;

                float amount = collisionCurrentDuration == 0
                    ? -25 * (collisionCurrentDuration - 4)
                    : -25 * (Mathf.Pow(2, collisionCurrentDuration) - 4);

                if (PlayerHealth.TakeDamage(amount) <= 0)
                {
                    Kill();
                }
                break;
            case RegionEffect.Unstable:
                Kill();
                break;
        }
    }

    public void Kill()
    {
        PlayerHealth.Kill();

        Debug.Log(name + " has died.");
        if (characterAnimation != null)
            characterAnimation.SetAnimation("Die");
        deathCountdown = 2f;
    }

    // Checks distance between this and other player
    // If within distance and both players make noise, will heal 1HP
    private void CoopHeal()
    {
        Vector3 distance = otherPlayer.transform.position - transform.position;
        if (distance.sqrMagnitude < maxActionDistance)
        {
            // If players are within MaxActionDistance...
        }
    }
}
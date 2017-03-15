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
    public float movementSpeed = 10f;
    // max distance player must be to interact with object
    public GameObject startingRegion;

    public PlayerID player;

    public Region Region
    {
        get { return currentRegion; }
    }

    // Temporary way to assign and access the two characters (??)
    public AnimatedCharacter characterAnimation;


    public GameObject otherPlayer;
    // Current game board region of the player
    private Region currentRegion;

    private PlayerHealth playerHealth;
    private int actionDistance = 1;

    // Use this for initialization
    void Start()
    {
        if (player == PlayerID.Both) throw new System.Exception("Invalid player name for control script");
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null) Debug.LogError(player.ToString() + "Missing PlayerHealth component");


        // identify other player
        otherPlayer = player == PlayerID.P1
            ? GameObject.Find("Player2")
            : player == PlayerID.P2 ? GameObject.Find("Player1") : null;

        characterAnimation = GetComponentInChildren<AnimatedCharacter>();
        if (characterAnimation == null)
            throw new Exception("This player object does not have a child with AnimatedCharacter.");

        Spawn();
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
    }

    // Use trigger callbacks to change the state of the character
    void OnTriggerEnter(Collider other)
    {
        if (gameObject.activeSelf && other.gameObject.layer == LayerMask.NameToLayer("Regions") && (currentRegion == null || other.transform != currentRegion.transform))
            changeRegion(other.transform);
    }

    private void changeRegion(Transform other)
    {
        if (currentRegion != null)
            currentRegion.SetOccupied(false, transform);
        currentRegion = other.GetComponent<Region>();
        Debug.Log(player.ToString() + " changing region to " + currentRegion.gameObject.name);
        currentRegion.SetOccupied(true, transform);
    }

    public void Spawn() {
        transform.position = startingRegion.GetComponent<Region>()[0].transform.position + 2 * Vector3.up;
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        changeRegion(startingRegion.transform);
        currentRegion.State = (player == PlayerID.P1) ? RegionState.A : RegionState.B;
    }
}
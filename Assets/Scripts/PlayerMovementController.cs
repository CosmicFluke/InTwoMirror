using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerMovementController : MonoBehaviour
{
    public float movementSpeed = 10f;

    // Temporary way to assign and access the two characters (??)
    public AnimatedCharacter characterAnimation;
    public GameObject otherPlayer;

    // Current game board region of the player
    private Region currentRegion;
    private PlayerID playerID;
    private int actionDistance = 1;

    // Use this for initialization
    void Start()
    {
        playerID = GetComponent<Player>().playerID;

        // identify other player
        otherPlayer = (playerID == PlayerID.P1)
            ? GameObject.Find("Player2")
            : (playerID == PlayerID.P2) ? GameObject.Find("Player1") : null;

        characterAnimation = GetComponentInChildren<AnimatedCharacter>();
        if (characterAnimation == null)
            throw new Exception("This player object does not have a child with AnimatedCharacter.");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 movement = new Vector3(Input.GetAxis(playerID.ToString() + "Horizontal"), 0f,
            Input.GetAxis(playerID.ToString() + "Vertical"));
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
            GetComponent<Player>().ChangeRegion(other.transform);
    }

}
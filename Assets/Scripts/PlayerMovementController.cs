﻿﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerID { P1, P2, Both };

public class PlayerMovementController : MonoBehaviour
{

    public float movementSpeed = 30f;
    // max distance player must be to interact with object
    public float maxActionDistance = 10f;

    public PlayerID player;
    private GameObject otherPlayer;

    public float healthPoints = 100f;

    // Current game board region of the player
    private RegionBuilder currentRegion;

    // Temporary way to assign and access the two characters
    public AnimatedCharacter characterAnimation;

    // Keeps track of volatile collision duration
    private float collisionStartTime;
    private float collisionCurrentDuration;
    private float collisionTotalDuration = 0f;

    //public UnityEngine.UI.Text CollisionText; 

    // Use this for initialization
    void Start()
    {
        if (player == PlayerID.Both) throw new System.Exception("Invalid player name for control script");

        // identify other player
        otherPlayer = player == PlayerID.P1 ? GameObject.Find("Player2") : player == PlayerID.P2 ? GameObject.Find("Player1") : null;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 movement = new Vector3(Input.GetAxis(player.ToString() + "Horizontal"), 0f, Input.GetAxis(player.ToString() + "Vertical"));
        Rigidbody rb = GetComponent<Rigidbody>();
        if (movement.magnitude > 0)
        {
            rb.velocity = movement * movementSpeed;

            // Rotate the character towards the direction of movement
            Quaternion newRotation = new Quaternion();
            newRotation.SetLookRotation(movement);
            transform.rotation = newRotation;

            // Call SetAnimation with parameter "Yell" to play the character's yelling animation
            if (characterAnimation != null) characterAnimation.SetAnimation("Run");
        } else {
            if (characterAnimation != null) characterAnimation.SetAnimation("Idle");
            rb.velocity = rb.velocity + (movement * movementSpeed);
        }
    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        changeRegion(other);
            
        collisionStartTime = Time.time;
    }

    private void changeRegion(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Regions"))
        {
            RegionBuilder r = other.GetComponent<RegionBuilder>();
            if (r == null) return;
            currentRegion = r;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<RegionBuilder>() != null)
        {
            ExecuteTileEffect(other.gameObject);
        }
    }

    private void OnTriggerExit()
    {
        collisionTotalDuration = collisionCurrentDuration;
    }

    private void ExecuteTileEffect(GameObject tile)
    {
        // Take damage from vola(tiles)^2
        if (RegionBuilder.StateToEffect(tile.GetComponent<RegionBuilder>().State, player) == RegionEffect.Volatile)
        {
            collisionCurrentDuration = collisionTotalDuration + Time.time - collisionStartTime;
            //CollisionText.text = collisionCurrentDuration.ToString();

            healthPoints = collisionCurrentDuration==0 ? -25 * (collisionCurrentDuration - 4) : -25 * (Mathf.Pow(2, collisionCurrentDuration) - 4);
            if (healthPoints <= 0)
            {
                Debug.Log(name + " has died.");
                GameObject.FindWithTag("LevelController").GetComponent<LevelController>().UpdatePlayerHealth(player, 0f);
            }
            else
            {
                Debug.Log(name + " HP = " + healthPoints + " at collision duration: " + collisionCurrentDuration.ToString());
                GameObject.FindWithTag("LevelController").GetComponent<LevelController>().UpdatePlayerHealth(player, healthPoints);
            }
        }
    }

    // Find the closest interactive object
    // From https://docs.unity3d.com/ScriptReference/GameObject.FindGameObjectsWithTag.html
    // Not currently in use
    GameObject FindClosestInteractive()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Interactive");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < maxActionDistance && curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
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
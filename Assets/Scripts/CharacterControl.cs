using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerID { P1, P2, Both };

public class CharacterControl : MonoBehaviour
{

    public float movementSpeed = 30f;
    // max distance player must be to interact with object
    public float maxActionDistance = 10f;

    public PlayerID player;

    // the closest actionable object to the player
    private GameObject actionable;

    // Secondary audio for this player
    private AudioSource audioSource;
    // Currently selected Camelot tone to play
    private int currCamelot;
    private List<AudioClip> camelotList;
    private Collider proximity = null;
    private GameObject otherPlayer;


    public int HealthPoints;

    // Current game board region of the player
    private GameObject currentRegion;

    // Temporary way to assign and access the two characters
    public AnimatedCharacter character;

    // Use this for initialization
    void Start()
    {
        actionable = null;
        audioSource = GetComponent<AudioSource>();
        currCamelot = 2;

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
            if (character != null) character.SetAnimation("Run");
        } else {
            if (character != null) character.SetAnimation("Idle");
            rb.velocity = rb.velocity + (movement * movementSpeed);
        }
    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger && other.CompareTag("Interactive"))
        {
            GetComponentInChildren<SoundControlScriptPd>().Interactive = other;
            actionable = other.gameObject;
        }
        if (other.gameObject.GetComponent<Region>() != null)
            ExecuteTileEffect(other.gameObject);
    }

    private void ExecuteTileEffect(GameObject tile)
    {
        // Take damage from vola(tiles)^2
        if (Region.StateToEffect(tile.GetComponent<Region>().State, player) == RegionEffect.Volatile)
        {
            HealthPoints--;
            Debug.Log(name + " HP = " + HealthPoints);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other == actionable)
        {
            actionable = null;
            GetComponentInChildren<SoundControlScriptPd>().Interactive = null;
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
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

    // Current game board region of the player
    private GameObject currentRegion;

    // Use this for initialization
    void Start()
    {
        actionable = null;
        audioSource = GetComponent<AudioSource>();
        if (player == PlayerID.Both) throw new System.Exception("Invalid player ID for control script");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 movement = new Vector3(Input.GetAxis(player.ToString() + "Horizontal"), 0f, Input.GetAxis(player.ToString() + "Vertical"));
        if (movement.magnitude > 0) {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(movement * movementSpeed);
        }
    }

    void Update() {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger && other.CompareTag("Interactive")) {
            GetComponentInChildren<SoundControlScriptPd>().Interactive = other;
            actionable = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == actionable) {
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
}

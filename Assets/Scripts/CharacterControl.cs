using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerID { P1, P2 };

public class CharacterControl : MonoBehaviour
{

    public float movementSpeed = 5f;
    // max distance player must be to interact with object
    public float maxActionDistance = 10f;

    // Contains audio source and sound control script
    public GameObject audioEmitter;

    // Audio clips
    public AudioClip secondarySound;
    public AudioClip CamelotTone0;
    public AudioClip CamelotTone1;
    public AudioClip CamelotTone2;
    public AudioClip CamelotTone3;
    public AudioClip CamelotTone4;

    // the closest actionable object to the player
    private GameObject actionable;
    // Secondary audio for this player
    private AudioSource audio;
    // Currently selected Camelot tone to play
    private int currCamelot;
    private List<AudioClip> camelotList;
    private PlayerID player;
    private Collider proximity = null;

    // Use this for initialization
    void Start()
    {
        actionable = null;
        audio = GetComponent<AudioSource>();
        currCamelot = 2;

        // Add tones to the list
        camelotList = new List<AudioClip>();
        camelotList.Add(CamelotTone0);
        camelotList.Add(CamelotTone1);
        camelotList.Add(CamelotTone2);
        camelotList.Add(CamelotTone3);
        camelotList.Add(CamelotTone4);
        audio.clip = camelotList[0];
        player = name == "Player1" ? PlayerID.P1 : PlayerID.P2;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 movement = new Vector3(Input.GetAxis(player.ToString() + "Horizontal"), 0f, Input.GetAxis(player.ToString() + "Vertical"));
        if (movement.magnitude > 0) {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(movement.normalized * 15);
        }

        // Object interaction
        actionable = FindClosestInteractive();
        if (actionable != null)
        {
            // Do something with the actionable object!
            Debug.Log("Closest actionable: " + actionable.name);

            // Pressing F plays audio
            if (Input.GetKeyUp(KeyCode.F))
            {
                audio.clip = secondarySound;
                audio.Play();
            }
        }
    }

    // Find the closest interactive object
    // From https://docs.unity3d.com/ScriptReference/GameObject.FindGameObjectsWithTag.html
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger && other.CompareTag("Interactive")) {
            GetComponentInChildren<SoundControlScriptPd>().Interactive = other;
            proximity = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == proximity) {
            proximity = null;
            GetComponentInChildren<SoundControlScriptPd>().Interactive = null;
        }
    }
}

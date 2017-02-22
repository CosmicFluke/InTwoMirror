using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    public float movementSpeed = 5f;
    // max distance player must be to interact with object
    public float maxActionDist = 10f;
    // the closest actionable object to the player
    private GameObject actionable;
    // Secondary audio for this player
    AudioSource audio;

    // Use this for initialization
    void Start () {
        actionable = null;
        audio = GetComponent<AudioSource>();

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * movementSpeed, Space.World);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * movementSpeed, Space.World);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * Time.deltaTime * movementSpeed, Space.World);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * movementSpeed, Space.World);
        }

        // Object interaction
        actionable = FindClosestInteractive();
        if(actionable != null)
        {
            // Do something with the actionable object!
            Debug.Log("Closest actionable: " +  actionable.name);

            // Pressing F plays audio
            if (Input.GetKeyUp(KeyCode.F))
            {
                audio.Play();
            }
        }
    }

    // Find the name of the closest enemy
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
            if (curDistance < maxActionDist && curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCameraShot : MonoBehaviour {

    public float maxRadiansPerSecond = 0.5f;
    public Vector3 rotateAround = new Vector3();

    float rotationAmt = 0.001f;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        rotationAmt = maxRadiansPerSecond * Time.deltaTime;
        transform.RotateAround(rotateAround, Vector3.up, Mathf.Rad2Deg * rotationAmt);
	}
}

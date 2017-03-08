using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCameraShot : MonoBehaviour {

    public float maxRadiansPerSecond = 1f;
    public Vector3 rotateAround = new Vector3();
    public float accelerationFactor = 1.05f;

    float rotationAmt = 0.05f;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        float t = Time.deltaTime;
        transform.RotateAround(rotateAround, Vector3.up, rotationAmt);
        if (rotationAmt < maxRadiansPerSecond * t) rotationAmt = Mathf.Min(maxRadiansPerSecond * t, rotationAmt + accelerationFactor * t);
	}
}

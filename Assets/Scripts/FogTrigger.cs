using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision collision) {
		foreach (GameObject fog in GameObject.FindGameObjectsWithTag("ThickFog")) {
			FogController.FadeOutFog (fog);
		}
	}

}

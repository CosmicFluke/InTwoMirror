using System;
using UnityEngine; 

public class CharControllerMonica : MonoBehaviour {

	private PlayerID player;

	public float movementSpeed = 30f;

	void Start(){
		
		player = (name == "Player1") ? PlayerID.P1 : PlayerID.P2;

	}

	void FixedUpdate(){
		
		Vector3 movement = new Vector3 (Input.GetAxis (player.ToString () + "Horizontal"), 0f, Input.GetAxis (player.ToString () + "Vertical"));
		if (movement.magnitude > 0) {
			Rigidbody rb = GetComponent<Rigidbody> ();
			rb.AddForce (movement * movementSpeed);
		}
	}

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GirlPlayer : MonoBehaviour {

	private Animator animator;

	// Use this for initialization
	void Start () {
		animator = gameObject.GetComponentInChildren<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey ("up")) {
			animator.SetInteger ("AnimPar", 1);
		} else if (Input.GetKey ("q")) {
			animator.SetInteger ("AnimPar", 2);
		} else {
			animator.SetInteger("AnimPar", 0);
		}
	}
}

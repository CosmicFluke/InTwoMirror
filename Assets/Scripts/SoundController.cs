using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
	bool isPlaying = false;
	bool isStopping = false;

	public AudioSource sound;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (sound.isPlaying) {
			GetComponent<ParticleSystem> ().Play ();
		} else {
			stopSound ();
		}
	}

	void startSound (AudioClip clip)
	{
		sound.clip = clip;
		sound.Play ();

		notifyInteractive ();
	}

	void stopSound ()
	{
		GetComponent<ParticleSystem> ().Stop ();
	}

	void notifyInteractive ()
	{
		// TODO: This may just better belong in the player controller script where the sounds are played from.
	}
		
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
	bool isPlaying = false;
	bool isStopping = false;

	public AudioSource sound;

	ParticleSystem particleSystem;

	// Use this for initialization
	void Start ()
	{
		particleSystem = GetComponent<ParticleSystem> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (sound.isPlaying) {
			particleSystem.Play ();
		} else {
			stopSound ();
		}
	}

	void startSound (AudioClip clip)
	{
		startSound (clip, GetComponent<ParticleSystem> ());
	}

	void startSound (AudioClip clip, ParticleSystem particleSystem)
	{
		this.particleSystem = particleSystem;

		sound.clip = clip;
		sound.Play ();

		notifyInteractive ();
	}

	void stopSound ()
	{
		particleSystem.Stop ();
	}

	void notifyInteractive ()
	{
		// TODO: This may just better belong in the player controller script where the sounds are played from.
	}
		
}

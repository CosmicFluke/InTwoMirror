using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    private AudioSource audioSource;

    public SoundController(AudioSource audioSource)
    {
        this.audioSource = audioSource;
    }

    // Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void startSound (AudioClip clip)
	{
        audioSource.clip = clip;
        audioSource.Play();
    }
}

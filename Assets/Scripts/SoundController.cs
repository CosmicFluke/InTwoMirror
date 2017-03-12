using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundController : MonoBehaviour
{
    public AudioClip clip;

    public bool IsPlaying { get { return isPlaying; } }

    private bool isPlaying = false;

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
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            Debug.Log("Could not start sound.  No audio source found.");
            return;
        }
        audioSource.clip = clip;
        audioSource.Play();
        isPlaying = true;
    }
}

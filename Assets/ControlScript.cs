using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlScript : MonoBehaviour {

    public Text uiText;
    public Text mouseYText;
    public AudioSource sound;
    public Image wheelNeedle;
    public AudioClip[] tones;

    AudioSource audioSource;
    int position = 0;
    int sampleRate = 0;
    float frequency = 0;

    string[] toneLabels = {"A", "B", "C#", "D", "E", "F#", "G"};
    int numTones = 7;

    /** The current tone rotation value (in degrees) */
    float toneRotation = 0f;

    /** Values for procedural sound (not used) */
    float baseFreq = 440f;
    float semitoneMultiplier = 1.059463094359f;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButton("Fire2")){
            Vector2 toneVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            
            //mouseYText.text = angleDegrees.ToString();
            //int currTone = (int) Mathf.Min(Mathf.Floor(angleDegrees) / 51.42f, 6);
            //sound.clip = tones[currTone];
            
            //uiText.text = toneLabels[currTone];
        }
        if (Input.GetButtonDown("Fire1"))
        {
            sound.Play();
        }
        if (Input.GetButtonUp("Fire1")) {
            sound.Stop();
        }

	}

    void OnAudioRead(float[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            
            data[i] = (Mathf.PingPong(frequency * position / sampleRate, 0.5f));
            position++;
        }
    }

    void OnAudioSetPosition(int newPosition)
    {
        position = newPosition;
    }

}

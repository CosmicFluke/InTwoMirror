using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlScript : MonoBehaviour {

    public Text uiText;
    public Text mouseYText;
    public AudioSource sound;
    public AudioClip[] tones;

    AudioSource audioSource;
    int position = 0;
    int sampleRate = 0;
    float frequency = 0;

    string[] toneLabels = {"A", "B", "C#", "D", "E", "F#", "G"};
    int numTones = 7;
    int currTone = 0;
    float innerPos = 0f;
    int selectionCooldown = 10;
    int cooldownCounter = -1;

    float baseFreq = 440f;
    float semitoneMultiplier = 1.059463094359f;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButton("Fire2") && cooldownCounter == -1) {
            float mouseY = Input.GetAxis("Mouse Y");
            mouseYText.text = mouseY.ToString();
            innerPos += mouseY;
            if (innerPos > 10) {
                currTone = (numTones + currTone + 1) % numTones;
                sound.clip = tones[currTone];
                cooldownCounter = selectionCooldown;
            } else if (innerPos < -10) {
                currTone = (numTones + currTone - 1) % numTones;
                sound.clip = tones[currTone];
                cooldownCounter = selectionCooldown;
            }
            uiText.text = toneLabels[currTone];
        }
        if (Input.GetButtonDown("Fire1"))
        {
            sound.Play();
        }
        if (Input.GetButtonUp("Fire1")) {
            sound.Stop();
        }

        if (cooldownCounter > -1) cooldownCounter--;

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

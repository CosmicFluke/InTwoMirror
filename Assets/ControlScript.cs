using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlScript : MonoBehaviour {

    public Text uiText;
    public Text toneAngleText;
    public AudioSource sound;
    public Image wheelNeedle;
    public AudioClip[] tones;
    public float toneVectorThreshold = 0.01f;
    public bool useController = true;

    AudioSource audioSource;
    bool playingSound = false;
    float currPitch = 1f;
    int currVolume = 1;

    string[] toneLabels = {"A", "Bb", "B", "C", "C#", "D", "Eb", "E", "F", "F#", "G", "G#"};
    int numTones = 7;

    /** The current tone rotation value (in degrees) */
    //float toneRotation = 0f;

    /** Values for procedural sound (not used) */
    //float baseFreq = 440f;
    float semitoneMultiplier = 1.059463094359f;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector2 toneVector = useController ? getJoystickInput() : getMouseInput();
        if (toneVector.magnitude > toneVectorThreshold){
            toneVector.Normalize();
            float toneAngle = Mathf.Rad2Deg * Mathf.Atan2(toneVector.y, toneVector.x);
            if (toneAngle < 0) {
                toneAngle += 360;
            }
            toneAngleText.text = toneAngle.ToString();
            float newPitch = 1 + toneAngle / 360;
            newPitch = Mathf.Pow(semitoneMultiplier, Mathf.Floor(Mathf.Log(newPitch, semitoneMultiplier)));
            if (currPitch != newPitch) {
                currPitch = newPitch;
                uiText.text = toneLabels[Mathf.FloorToInt(Mathf.Log(currPitch, semitoneMultiplier))];
                sound.pitch = newPitch;
            }
        }
        float soundVol = Input.GetAxis("MakeSound");
        if (!playingSound && soundVol > 0)
        {
            startSound();

        }
        if (soundVol == 0) {
            stopSound();
        }

	}

    Vector2 getJoystickInput() {
        return new Vector2(-Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    Vector2 getMouseInput() {
        float wheelVal = Input.GetAxis("Mouse ScrollWheel");
        float toneAngleRad = ((Mathf.Abs(wheelVal) > 0 ? currPitch * (10 * wheelVal) : currPitch) - 1) * 2 * Mathf.PI;
        Vector2 vector = new Vector2(Mathf.Cos(toneAngleRad), Mathf.Sin(toneAngleRad));
        Debug.Log(vector);
        return vector;
    }

    void startSound() {
        sound.volume = 0;
        sound.Play();
        playingSound = true;
        for (int i = 1; i <= 1000; i++)
            sound.volume = currVolume * i / 1000;
    }

    void stopSound() {
        for (int i = 999; i >= 0; i--)
            sound.volume = currVolume * i / 1000;
        sound.Stop();
        playingSound = false;
    }

}

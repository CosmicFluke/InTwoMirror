using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Keys { Chromatic, AMajor, AMinor, DMajor, FSharpMinor, CMajor, EMinor };

public class SoundControlScriptPd : MonoBehaviour {

    // Required
    public AudioClip tone; // tones[0] required as base tone for pitch adjustment
    public Image wheelNeedle;
    
    public float toneVectorThreshold = 0.25f;
    public bool useController = true;

    public GameObject dependencies;

    AudioSource sound;
    GameObject soundProducer;
    bool playingSound = false;
    int currNoteOffset = 0;
    int currOctave = 1;
    int currVolume = 1;
    int currKey = 0;

    const int baseNote = 57;
    string[] toneLabels = {"A", "Bb", "B", "C", "C#", "D", "Eb", "E", "F", "F#", "G", "G#"};
    int numTones = 7;

    /** The current tone rotation value (in degrees) */
    //float toneRotation = 0f;

    /** Values for procedural sound (not used) */
    //float baseFreq = 440f;
    //float semitoneMultiplier = 1.059463094359f;

    // Use this for initialization
    void Start () {
        sound = GetComponent<AudioSource>();
        sound.clip = tone;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        Hv_ObeliskVoice_v1_AudioLib selfVoice = GetComponent<Hv_ObeliskVoice_v1_AudioLib>();

        int keyAdjust = DPadButtons.right ? 1 : DPadButtons.left ? -1 : 0;
        Vector2 offsetVector = useController ? getJoystickInput() : getMouseInput();

        if (DPadButtons.up) {
            currOctave = Mathf.Abs(currOctave - 1);
            // TODO: GUI Adjust
        }

        if (keyAdjust != 0) {
            currKey = currKey + keyAdjust;
            // TODO: GUI Adjust
        }

        if (offsetVector.magnitude > toneVectorThreshold){
            offsetVector.Normalize();
            float toneAngle = Mathf.Rad2Deg * Mathf.Atan2(offsetVector.y, offsetVector.x);
            currNoteOffset = angleToOffset(toneAngle);
            setWheelNeedle(toneAngle);
        }

        setPitch(selfVoice);

        float makeSound = Input.GetAxis("MakeSound");
        if (!playingSound && makeSound > 0)
            startSound();
        if (playingSound && makeSound == 0)
            stopSound();

	}

    Vector2 getJoystickInput() {
        return new Vector2(-Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    Vector2 getMouseInput() {
        float wheelVal = Input.GetAxis("Mouse ScrollWheel");
        // Convert current note offset into a radial value & add wheel input value if necessary
        float toneAngleRad = (Mathf.Abs(wheelVal) > 0 ? (currNoteOffset + 10 * wheelVal) % 12 : currNoteOffset) * 2 * Mathf.PI / 12;
        Vector2 vector = new Vector2(Mathf.Cos(toneAngleRad), Mathf.Sin(toneAngleRad));
        return vector;
    }

    void setWheelNeedle(float toneAngle)
    {
        if (wheelNeedle != null)
            wheelNeedle.transform.rotation = Quaternion.AngleAxis(-toneAngle, Vector3.forward);
    }

    int angleToOffset(float toneAngle)
    {
        if (toneAngle < 0)
            toneAngle += 360;
        return Mathf.FloorToInt(12 * toneAngle / 360);
    }

    void setPitch(Hv_ObeliskVoice_v1_AudioLib voice)
    {
        float newPitch = baseNote + 12 * currOctave + currNoteOffset;

        if (newPitch != voice.GetFloatParameter(Hv_ObeliskVoice_v1_AudioLib.Parameter.Pitch))
        {
            string toneName = toneLabels[currNoteOffset];
            voice.SetFloatParameter(Hv_ObeliskVoice_v1_AudioLib.Parameter.Pitch, newPitch);
            if (playingSound)
            {
                stopSound();
                startSound();
            }
        }
    }

    void startSound() {
        sound.volume = 0;
        sound.Play();
        playingSound = true;
    }

    void stopSound() {
        sound.Stop();
        playingSound = false;
    }

}

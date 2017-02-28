using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundControlScriptPd : MonoBehaviour {

    // Required
    public AudioClip tone; // tone not required, not currently used
    public Image wheelNeedle;
    
    public float toneVectorThreshold = 0.25f;
    public bool useController = false;
    public PlayerID player;

    public Collider Interactive {
        get { return proximity; }
        set { proximity = value; }
    }

    private Collider proximity = null;
    AudioSource sound;
    GameObject soundProducer;
    bool playingSound = false;
    int currNoteOffset = 0;
    int currOctave = 1;
    int currVolume = 1;
    int currKey = 0;

    const int baseNote = 57;
    int numTones = 7;

    public int Pitch { get { return baseNote + 12 * currOctave + currNoteOffset; } }

    // Use this for initialization
    void Start () {
        sound = GetComponent<AudioSource>();
        if (tone != null) sound.clip = tone;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        Hv_ObeliskVoice_v1_AudioLib selfVoice = GetComponent<Hv_ObeliskVoice_v1_AudioLib>();

        int keyAdjust = DPadButtons.right ? 1 : DPadButtons.left ? -1 : 0;
        Vector2 offsetVector = useController ? getJoystickInput() : player == PlayerID.P1 ? getTwoButtonPitchInput() : getOneAxisPitchInput();

        if (Input.GetButtonDown(player.ToString() + "ToggleOctave")) {
            currOctave = Mathf.Abs(currOctave - 1);
            ParticleSystem.MainModule parts = GetComponent<ParticleSystem>().main;
            parts.startColor = new ParticleSystem.MinMaxGradient(currOctave == 0 ? Color.blue : Color.white);
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

        float makeSound = Input.GetAxis(player.ToString() + "MakeSound");
        if (!playingSound && makeSound > 0)
            startSound();
        if (playingSound && makeSound == 0)
            stopSound();

	}

    Vector2 getJoystickInput() {
        return new Vector2(-Input.GetAxis(player.ToString() + "PitchX"), Input.GetAxis(player.ToString() + "PitchY"));
    }

    /** Calculate a pitch vector from button input when controller is not enabled.  Currently used only for Player 1 since Player 2 has the mouse scroll wheel. */
    Vector2 getTwoButtonPitchInput() {
        int change = Input.GetButtonDown(player.ToString() + "PitchUp") ? 1 : Input.GetButtonDown(player.ToString() + "PitchDown") ? -1 : 0;
        return toneAngleToPitchVector(offsetToAngle(currNoteOffset + change));
    }

    /** Calculate a pitch vector from a single-axis input when controller is not enabled.  Currently used only with the mouse scroll wheel for Player 2. */
    Vector2 getOneAxisPitchInput() {
        float pitchAxis = Input.GetAxis(player.ToString() + "Pitch");
        // Convert current note offset into a radial value & add axis input value if necessary
        int offset = Mathf.Abs(pitchAxis) > 0 ? (currNoteOffset + Mathf.FloorToInt(Mathf.Min(1f, 10 * pitchAxis))) % 12 : currNoteOffset;
        return toneAngleToPitchVector(offsetToAngle(offset));
    }

    float offsetToAngle(int offset)
    {
        return offset * Mathf.PI / 6;
    }

    Vector2 toneAngleToPitchVector(float toneAngleRad) {
        return new Vector2(Mathf.Cos(toneAngleRad), Mathf.Sin(toneAngleRad));
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
        GetComponent<ParticleSystem>().Play();
        if (proximity != null)
        {
            proximity.gameObject.GetComponent<ResonatorController>()
                .activate(transform.parent.gameObject, Pitch);
        }
    }

    void stopSound() {
        sound.Stop();
        playingSound = false;
        GetComponent<ParticleSystem>().Stop();
    }

}

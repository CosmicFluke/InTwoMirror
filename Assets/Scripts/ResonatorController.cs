using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResonatorController : MonoBehaviour {

    public Tone tone;
    public Chord chord;
    public float oscillatingFrequency = 0f;
    public bool distort;
    public Material activeMaterial;
    public float glowIntensity = 7f;
    public PlayerID player;
    public float fadeFactor = 0.5f;

    bool isPlaying = false;
    bool isStopping = false;
    GameObject playerObj;
    Key targetKey;
    const int baseNote = 57;
    Material baseMaterial;
    int ossCounter = 0;

    // PlayerID.Both variables
    int playerTone;
    int secondTone;

    // Use this for initialization
    void Start () {
        targetKey = new Key(tone, chord);
        MeshRenderer mrend = GetComponentInChildren<MeshRenderer>();
        baseMaterial = mrend.material;
    }
	
	// Update is called once per frame
	void Update () {
        if (isPlaying && oscillatingFrequency > 0)
        {
            Hv_ObeliskVoice_v1_AudioLib audio = GetComponent<Hv_ObeliskVoice_v1_AudioLib>();
            audio.SetFloatParameter(Hv_ObeliskVoice_v1_AudioLib.Parameter.Mastervoicevolume, 0.6f * Mathf.Abs(Mathf.Sin(ossCounter * oscillatingFrequency * Mathf.PI / 60)) + 0.25f);
            ossCounter++;
        } else ossCounter = 0;
        AudioSource sound = GetComponent<AudioSource>();
        if (isPlaying && sound.volume < 1) {
            sound.volume = sound.volume + fadeFactor * Time.deltaTime;
            Light glow = GetComponentInChildren<Light>();
        if (isPlaying && glow.intensity < glowIntensity)
            glow.intensity = glow.intensity + (fadeFactor * glowIntensity) * Time.deltaTime;
        }

        if (isStopping)
        {
            fadeOut();
        }
		
	}

    void start() {
        if (isPlaying) return;
        AudioSource sound = GetComponent<AudioSource>();
        if (isStopping)
            isStopping = false;
        else
        {
            sound.volume = 0;
            sound.Play();
        }
        isPlaying = true;
        MeshRenderer mrend = GetComponentInChildren<MeshRenderer>();
        mrend.material = activeMaterial;
        mrend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    void fadeOut()
    {
        AudioSource sound = GetComponent<AudioSource>();
        if (sound.volume > 0)
            sound.volume = sound.volume - fadeFactor * Time.deltaTime;
        Light glow = GetComponentInChildren<Light>();
        if (glow.intensity > 0)
            glow.intensity = glow.intensity - (fadeFactor * glowIntensity) * Time.deltaTime;

        if (glow.intensity <= 0 && sound.volume <= 0)
        {
            isStopping = false;
            sound.Stop();
        }
    }

    void stop() {
        if (!isPlaying) return;
        isStopping = true;
        isPlaying = false;
        MeshRenderer mrend = GetComponentInChildren<MeshRenderer>();
        mrend.material = baseMaterial;
        mrend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    public void activate(GameObject activatingPlayer, int tone) {
        // Limit activation to specified player (if one-player resonator)
        if (player != PlayerID.Both && activatingPlayer.GetComponentInChildren<SoundControlScriptPd>().player != player) return;

        // For one-player resonators
        if (player != PlayerID.Both && checkTone(tone))
        {
            Hv_ObeliskVoice_v1_AudioLib audio = GetComponent<Hv_ObeliskVoice_v1_AudioLib>();
            audio.SetFloatParameter(Hv_ObeliskVoice_v1_AudioLib.Parameter.Pitch, tone);
            playerObj = activatingPlayer;
            start();
        }
        // For two-player resonators
        else if (player == PlayerID.Both) {
            // First player to activate
            if (playerObj == null && checkTone(tone)) {
                playerObj = activatingPlayer;
                playerTone = tone;
                // halfStart();
            }
            // Second player to activate
            else if (playerObj != null && playerTone != tone && checkTone(tone)) {
                Hv_ObeliskVoice_v1_AudioLib audio = GetComponent<Hv_ObeliskVoice_v1_AudioLib>();
                audio.SetFloatParameter(Hv_ObeliskVoice_v1_AudioLib.Parameter.Pitch, baseNote + (int) this.tone);
                playerObj = activatingPlayer;
                start();
            }
        }
        else
        {
            stop();
        }
    }

    bool checkTone(int tone) {
        int[] offsets = HarmonyUtil.Offsets[targetKey.Chord];
        int tonic = (int) targetKey.Tone;
        int note = (tone - baseNote) % 12;
        
        int offset = (note - tonic + 12) % 12;
        Debug.Log("Checking: " + ((Tone) note).ToString() + " is " + offset.ToString() + " above " + ((Tone) tonic).ToString());
        foreach (int scaleOffset in offsets)
        {
            if (scaleOffset == offset) return true;
        }
        return false;
    }
}

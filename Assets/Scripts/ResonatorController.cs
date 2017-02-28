﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResonatorController : MonoBehaviour {

    public Tone tone;
    public Chord chord;
    public float oscillatingFrequency = 0f;
    public bool distort;
    public Material activeMaterial;
    public float glowIntensity = 7f;

    bool isPlaying = false;
    GameObject player;
    int playerTone;
    Key targetKey;
    const int baseNote = 57;
    Material baseMaterial;

    int ossCounter = 0;

	// Use this for initialization
	void Start () {
        targetKey = new Key(tone, chord);
        MeshRenderer mrend = GetComponentInChildren<MeshRenderer>();
        baseMaterial = mrend.material;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (isPlaying && oscillatingFrequency > 0)
        {
            Hv_ObeliskVoice_v1_AudioLib audio = GetComponent<Hv_ObeliskVoice_v1_AudioLib>();
            audio.SetFloatParameter(Hv_ObeliskVoice_v1_AudioLib.Parameter.Mastervoicevolume, 0.6f * Mathf.Abs(Mathf.Sin(ossCounter * oscillatingFrequency * Mathf.PI / 60)) + 0.25f);
            ossCounter++;
        } else ossCounter = 0;
		
	}

    void start() {
        if (isPlaying) return;
        AudioSource sound = GetComponent<AudioSource>();
        sound.Play();
        isPlaying = true;
        MeshRenderer mrend = GetComponentInChildren<MeshRenderer>();
        mrend.material = activeMaterial;
        mrend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Light glow = GetComponentInChildren<Light>();
        glow.intensity = glowIntensity;
    }

    void stop() {
        if (!isPlaying) return;
        AudioSource sound = GetComponent<AudioSource>();
        sound.Stop();
        isPlaying = false;
        MeshRenderer mrend = GetComponentInChildren<MeshRenderer>();
        mrend.material = baseMaterial;
        mrend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        Light glow = GetComponentInChildren<Light>();
        glow.intensity = 0;
    }

    public void activate(GameObject player, int tone) {
        if (checkTone(tone))
        {
            Hv_ObeliskVoice_v1_AudioLib audio = GetComponent<Hv_ObeliskVoice_v1_AudioLib>();
            audio.SetFloatParameter(Hv_ObeliskVoice_v1_AudioLib.Parameter.Pitch, tone);
            this.player = player;
            start();
        }
        else
        {
            stop();
        }
    }

    bool checkTone(int tone) {
        int[] offsets = HarmonyUtil.Offsets[targetKey.Chord];
        int tonic = (int) targetKey.Tone;

        foreach (int offset in offsets) if (Mathf.Abs((tone - baseNote) % 12 - tonic) == offset) return true;
        return false;
    }
}

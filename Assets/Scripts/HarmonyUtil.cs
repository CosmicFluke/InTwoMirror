using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Chord { Chromatic, Major, Minor, Dom7, Dim7 };
public enum Tone { A, BFlat, B, C, CSharp, D, EFlat, E, F, FSharp, G, GSharp }

public class Key {
    private Tone tone;
    private Chord chord;

    public Tone Tone { get { return tone; } }
    public Chord Chord { get { return chord; } }

    public Key(Tone tone, Chord chord) {
        this.tone = tone;
        this.chord = chord;
    }
}

public class HarmonyUtil : MonoBehaviour {

    private static readonly int[] majorOffsets = { 0, 2, 4, 5, 7, 9, 11 };
    private static readonly int[] minorOffsets = { 0, 2, 3, 5, 7, 8, 11 };
    private static readonly int[] dom7Offsets = { 0, 4, 7, 11 };
    private static readonly int[] dim7Offsets = { 0, 3, 8, 10 };

    public static Dictionary<Chord, int[]> Offsets {
        get {
            return new Dictionary<Chord, int[]> {
                { Chord.Chromatic, (from num in Enumerable.Range(0, 12) select num).ToArray() },
                { Chord.Major, Major },
                { Chord.Minor, Minor },
                { Chord.Dom7, Dominant },
                { Chord.Dim7, Diminished }
            };
        }
    }

    public static float semitoneMultiplier { get { return 1.059463094359f; } }

    public static int[] Major {
        get { return (int[]) majorOffsets.Clone(); }
    }

    public static int[] Minor {
        get { return (int[]) minorOffsets.Clone(); }
    }

    public static int[] Dominant {
        get { return (int[]) dom7Offsets.Clone(); }
    }

    public static int[] Diminished {
        get { return (int[]) dim7Offsets.Clone(); }
    }

    static float midiToFrequency(int midiNum)
    {
        return Mathf.Pow(2, (midiNum - 69) / 12) * 440;
    }

    static float frequencyToMidi(float freq)
    {
        return 69 + 12 * Mathf.Log(freq / 440) / Mathf.Log(2);
    }

}

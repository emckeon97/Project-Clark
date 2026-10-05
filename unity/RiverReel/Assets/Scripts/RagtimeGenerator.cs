using UnityEngine;

/// <summary>
/// Original ragtime-flavored loop (oom-pah bass + syncopated melody, C major),
/// rendered into an AudioClip at runtime. Used only when the real
/// "delta_ragtime.mp3" (Kevin MacLeod's "The Entertainer", CC-BY 4.0)
/// has not been dropped into Assets/Audio/Music/ yet.
/// </summary>
public static class RagtimeGenerator
{
    const int Rate = 22050;
    const float Beat = 0.40f;      // 2/4 at a jaunty clip
    const int Bars = 16;

    // chord roots (midi) per bar: C G7 C C | F C G7 C | C G7 C F | C G7 C C
    static readonly int[] Roots = { 36, 31, 36, 36, 41, 36, 31, 36, 36, 31, 36, 41, 36, 31, 36, 36 };

    static int[] ChordTones(int root)
    {
        if (root == 31) return new[] { 47, 50, 55 };  // G7: B2 D3 G3
        if (root == 41) return new[] { 45, 48, 53 };  // F:  A2 C3 F3
        return new[] { 48, 52, 55 };                  // C:  C3 E3 G3
    }

    // original melody, 4 eighth-note slots per bar (-1 = rest)
    static readonly int[] Melody =
    {
        76, 79, 81, 79,
        71, 74, 79, 77,
        76, -1, 79, 76,
        74, 76, 77, 76,
        69, 72, 77, 81,
        79, 76, 74, 72,
        71, 74, 77, 74,
        76, 79, -1, 84,
        84, 83, 81, 79,
        77, 74, 71, 74,
        76, 79, 81, 84,
        81, 79, 77, 76,
        74, 76, 79, 76,
        74, 77, 71, 74,
        76, -1, 74, 72,
        72, -1, -1, -1,
    };

    static float MidiToFreq(int m)
    {
        return 440f * Mathf.Pow(2f, (m - 69) / 12f);
    }

    // piano-ish voice: harmonic stack with exponential decay
    static void Pluck(float[] buf, float startSec, int midi, float dur, float vol)
    {
        float f = MidiToFreq(midi);
        int s0 = (int)(startSec * Rate);
        int n = (int)(dur * Rate);
        for (int i = 0; i < n; i++)
        {
            int idx = s0 + i;
            if (idx < 0 || idx >= buf.Length) continue;
            float t = i / (float)Rate;
            float env = Mathf.Exp(-t * 3.2f);
            float v = 0f;
            v += Mathf.Sin(2f * Mathf.PI * f * t);
            v += 0.45f * Mathf.Sin(2f * Mathf.PI * f * 2f * t);
            v += 0.22f * Mathf.Sin(2f * Mathf.PI * f * 3f * t);
            v += 0.10f * Mathf.Sin(2f * Mathf.PI * f * 4f * t);
            buf[idx] += v * env * vol * 0.5f;
        }
    }

    public static AudioClip GenerateLoop()
    {
        float total = Bars * 2f * Beat;
        int n = (int)(total * Rate);
        float[] buf = new float[n];

        for (int bar = 0; bar < Bars; bar++)
        {
            float barStart = bar * 2f * Beat;
            int root = Roots[bar];
            int[] tones = ChordTones(root);

            // oom-pah left hand: bass on beat 1, chord stab on beat 2
            Pluck(buf, barStart, root, 0.34f, 0.85f);
            foreach (int tone in tones) Pluck(buf, barStart + Beat, tone, 0.24f, 0.42f);

            // syncopated right hand
            for (int s = 0; s < 4; s++)
            {
                int m = Melody[bar * 4 + s];
                if (m < 0) continue;
                Pluck(buf, barStart + s * (Beat / 2f), m, 0.30f, 0.75f);
            }
        }

        // soft fade on the last 0.25s so the loop has no click
        int fade = (int)(0.25f * Rate);
        for (int i = 0; i < fade; i++)
        {
            int idx = n - 1 - i;
            if (idx >= 0) buf[idx] *= i / (float)fade;
        }

        // gentle tanh soft-clip
        for (int i = 0; i < n; i++)
            buf[i] = Mathf.Clamp(buf[i], -1f, 1f);

        var clip = AudioClip.Create("ragtime_loop", n, 1, Rate, false);
        clip.SetData(buf, 0);
        return clip;
    }
}

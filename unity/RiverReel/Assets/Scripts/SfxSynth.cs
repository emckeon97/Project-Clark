using System;
using UnityEngine;

/// <summary>
/// Synthesized sound effects — no binary audio required.
/// Short retro blips rendered into AudioClips at runtime.
/// </summary>
public static class SfxSynth
{
    public const int SampleRate = 22050;

    static AudioClip Render(float seconds, Func<float, float> fn)
    {
        int n = Mathf.Max(1, (int)(seconds * SampleRate));
        float[] data = new float[n];
        for (int i = 0; i < n; i++)
            data[i] = Mathf.Clamp(fn(i / (float)SampleRate), -1f, 1f);
        var clip = AudioClip.Create("sfx", n, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static float Noise()
    {
        return UnityEngine.Random.value * 2f - 1f;
    }

    /// <summary>Quick rising whoosh for a flap.</summary>
    public static AudioClip MakeFlap()
    {
        float phase = 0f;
        return Render(0.14f, t =>
        {
            float f = 380f + 2600f * t;
            phase += 2f * Mathf.PI * f / SampleRate;
            float env = Mathf.Exp(-t * 28f);
            return (Mathf.Sin(phase) * 0.55f + Noise() * 0.30f) * env;
        });
    }

    /// <summary>Two-note chime when a stack pair is cleared.</summary>
    public static AudioClip MakeScore()
    {
        return Render(0.32f, t =>
        {
            float f = t < 0.13f ? 880f : 1174.66f;
            float tt = t < 0.13f ? t : t - 0.13f;
            float env = Mathf.Exp(-tt * 11f);
            return Mathf.Sin(2f * Mathf.PI * f * tt) * env * 0.7f;
        });
    }

    /// <summary>Descending sad sweep for death.</summary>
    public static AudioClip MakeDie()
    {
        return Render(0.6f, t =>
        {
            float f = 520f - 720f * t;
            if (f < 60f) f = 60f;
            float env = Mathf.Exp(-t * 4.5f);
            return (Mathf.Sin(2f * Mathf.PI * f * t) * 0.65f + Noise() * Mathf.Exp(-t * 22f) * 0.45f) * env;
        });
    }

    /// <summary>Water splash: lowpassed noise burst.</summary>
    public static AudioClip MakeSplash()
    {
        float y = 0f;
        return Render(0.38f, t =>
        {
            y = 0.55f * y + 0.45f * Noise();
            return y * Mathf.Exp(-t * 8f) * 1.6f;
        });
    }

    /// <summary>UI click blip.</summary>
    public static AudioClip MakeClick()
    {
        return Render(0.07f, t =>
            Mathf.Sin(2f * Mathf.PI * 1500f * t) * Mathf.Exp(-t * 60f) * 0.45f);
    }
}

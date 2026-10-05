using UnityEngine;

/// <summary>
/// All game audio. Music prefers the real delta_ragtime.mp3 (via MusicLibrary)
/// and falls back to the procedural ragtime loop. SFX are always synthesized.
/// </summary>
public class AudioManager : MonoBehaviour
{
    AudioSource musicSource;
    AudioSource[] sfxSources;
    int sfxCursor;

    AudioClip flapClip, scoreClip, dieClip, splashClip, clickClip;

    public void Init()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.45f;
        musicSource.playOnAwake = false;

        sfxSources = new AudioSource[4];
        for (int i = 0; i < sfxSources.Length; i++)
        {
            sfxSources[i] = gameObject.AddComponent<AudioSource>();
            sfxSources[i].playOnAwake = false;
            sfxSources[i].volume = 0.8f;
        }

        AudioClip music = null;
        var lib = Resources.Load<MusicLibrary>("MusicLibrary");
        if (lib != null) music = lib.ragtimeClip;
        if (music == null) music = RagtimeGenerator.GenerateLoop();
        musicSource.clip = music;

        flapClip = SfxSynth.MakeFlap();
        scoreClip = SfxSynth.MakeScore();
        dieClip = SfxSynth.MakeDie();
        splashClip = SfxSynth.MakeSplash();
        clickClip = SfxSynth.MakeClick();
    }

    void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;
        var src = sfxSources[sfxCursor];
        sfxCursor = (sfxCursor + 1) % sfxSources.Length;
        src.clip = clip;
        src.Play();
    }

    public void StartMusic()
    {
        if (musicSource != null && musicSource.clip != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    public void PlayFlap() { PlayOneShot(flapClip); }
    public void PlayScore() { PlayOneShot(scoreClip); }
    public void PlayDie() { PlayOneShot(dieClip); }
    public void PlaySplash() { PlayOneShot(splashClip); }
    public void PlayClick() { PlayOneShot(clickClip); }
}

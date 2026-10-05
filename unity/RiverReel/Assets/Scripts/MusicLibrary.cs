using UnityEngine;

/// <summary>
/// Holds the licensed ragtime track. Auto-wired by the editor script when
/// delta_ragtime.mp3 lands in Assets/Audio/Music/. Lives in Assets/Resources.
/// </summary>
public class MusicLibrary : ScriptableObject
{
    public AudioClip ragtimeClip;
}

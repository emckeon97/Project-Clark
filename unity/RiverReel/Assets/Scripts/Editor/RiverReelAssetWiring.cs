#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Watches Assets/Sprites/Characters/ and Assets/Audio/Music/. When Elijah
/// drops the real PNGs / delta_ragtime.mp3 into the project, this wires them
/// into the CharacterLibrary / MusicLibrary assets in Assets/Resources/
/// (in GameConfig.CharacterFileNames order) so the game picks them up with
/// zero manual setup. Missing files simply stay null and the game falls back
/// to procedural placeholder art + synth audio.
/// </summary>
public class RiverReelAssetWiring : AssetPostprocessor
{
    const string CharDir = "Assets/Sprites/Characters";
    const string MusicDir = "Assets/Audio/Music";
    const string ResourcesDir = "Assets/Resources";
    const string CharacterLibraryPath = "Assets/Resources/CharacterLibrary.asset";
    const string MusicLibraryPath = "Assets/Resources/MusicLibrary.asset";

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool touched = false;
        foreach (var p in importedAssets)
            if (p.StartsWith(CharDir) || p.StartsWith(MusicDir)) { touched = true; break; }
        if (!touched)
            foreach (var p in deletedAssets)
                if (p.StartsWith(CharDir) || p.StartsWith(MusicDir)) { touched = true; break; }
        if (touched) WireLibraries();
    }

    [MenuItem("River Reel/Rewire Character + Music Libraries")]
    public static void WireLibraries()
    {
        if (!Directory.Exists(ResourcesDir))
            Directory.CreateDirectory(ResourcesDir);

        // characters, in canonical order
        var lib = AssetDatabase.LoadAssetAtPath<CharacterLibrary>(CharacterLibraryPath);
        if (lib == null)
        {
            lib = ScriptableObject.CreateInstance<CharacterLibrary>();
            AssetDatabase.CreateAsset(lib, CharacterLibraryPath);
        }
        lib.characterSprites = new List<Sprite>();
        foreach (var name in GameConfig.CharacterFileNames)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{CharDir}/{name}.png");
            lib.characterSprites.Add(sprite); // null stays null -> placeholder fallback
        }
        EditorUtility.SetDirty(lib);

        // music
        var music = AssetDatabase.LoadAssetAtPath<MusicLibrary>(MusicLibraryPath);
        if (music == null)
        {
            music = ScriptableObject.CreateInstance<MusicLibrary>();
            AssetDatabase.CreateAsset(music, MusicLibraryPath);
        }
        music.ragtimeClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{MusicDir}/delta_ragtime.mp3");
        EditorUtility.SetDirty(music);

        AssetDatabase.SaveAssets();
        int found = 0;
        foreach (var s in lib.characterSprites) if (s != null) found++;
        Debug.Log($"[River Reel] Wired {found}/{GameConfig.CharacterFileNames.Length} character sprites, " +
                  $"music: {(music.ragtimeClip != null ? music.ragtimeClip.name : "procedural fallback")}");
    }
}
#endif

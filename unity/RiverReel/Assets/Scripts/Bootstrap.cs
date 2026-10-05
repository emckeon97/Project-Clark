using UnityEngine;

/// <summary>
/// Entry point. The scene contains only a Main Camera, so the whole game
/// world is bootstrapped here at load time — no hand-authored scene
/// references to break.
/// </summary>
public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Start()
    {
        var root = new GameObject("RiverReel");
        Object.DontDestroyOnLoad(root);
        root.AddComponent<GameManager>();
    }
}

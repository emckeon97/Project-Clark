using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the 9 real character sprites in GameConfig.CharacterFileNames order.
/// Auto-wired by the editor script when PNGs land in Assets/Sprites/Characters/.
/// Lives in Assets/Resources so runtime code can find it without scene wiring.
/// </summary>
public class CharacterLibrary : ScriptableObject
{
    public List<Sprite> characterSprites = new List<Sprite>();

    public Sprite Get(int index)
    {
        if (index >= 0 && index < characterSprites.Count)
            return characterSprites[index];
        return null;
    }
}

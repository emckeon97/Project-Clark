using UnityEngine;

/// <summary>
/// Static tuning + content tables for River Reel.
/// Everything the game needs to know lives here so the procedural
/// builders stay dumb and the numbers stay in one place.
/// </summary>
public static class GameConfig
{
    // ---- Characters (filenames must match the PNGs he drops in) ----
    public static readonly string[] CharacterFileNames =
    {
        "felix", "popeye", "oswald", "koko", "bimbo",
        "pooh", "olive", "bosko", "pete"
    };

    public static readonly string[] CharacterDisplayNames =
    {
        "Felix", "Popeye", "Oswald", "Koko the Clown", "Bimbo",
        "Pooh", "Olive Oyl", "Bosko", "Peg-Leg Pete"
    };

    // Placeholder body/accent colors (used only until the real PNGs arrive).
    public static readonly Color[] CharacterBodyColors =
    {
        new Color(0.16f, 0.16f, 0.18f), // felix - near-black cat
        new Color(0.13f, 0.23f, 0.38f), // popeye - navy sailor
        new Color(0.12f, 0.14f, 0.24f), // oswald - dark blue rabbit
        new Color(0.92f, 0.90f, 0.86f), // koko - white clown face
        new Color(0.72f, 0.55f, 0.34f), // bimbo - tan pup
        new Color(0.85f, 0.62f, 0.25f), // pooh - golden bear
        new Color(0.93f, 0.88f, 0.78f), // olive - pale
        new Color(0.42f, 0.28f, 0.16f), // bosko - brown
        new Color(0.23f, 0.16f, 0.13f), // pete - dark bruiser
    };

    public static readonly Color[] CharacterAccentColors =
    {
        new Color(0.95f, 0.95f, 0.95f),
        new Color(0.95f, 0.95f, 0.95f),
        new Color(0.20f, 0.24f, 0.42f),
        new Color(0.85f, 0.15f, 0.12f),
        new Color(0.55f, 0.38f, 0.20f),
        new Color(0.70f, 0.45f, 0.15f),
        new Color(0.15f, 0.12f, 0.12f),
        new Color(0.80f, 0.20f, 0.16f),
        new Color(0.75f, 0.18f, 0.14f),
    };

    // ---- Physics / gameplay tuning ----
    public const float Gravity = -22f;
    public const float FlapVelocity = 7.6f;
    public const float MaxFallSpeed = -12.5f;
    public const float PlayerX = -2.2f;
    public const float PlayerHalfSize = 0.34f;

    public const float BaseScrollSpeed = 2.4f;
    public const float MaxScrollSpeed = 4.9f;
    public const float SpeedPerScore = 0.035f;

    public const float BaseGap = 2.75f;
    public const float MinGap = 1.85f;
    public const float GapPerScore = 0.022f;

    public const float StackSpacing = 3.4f;
    public const float StackWidth = 1.3f;
    public const float SpawnX = 7.5f;
    public const float DespawnX = -8.5f;

    public const float WaterY = -4.15f;
    public const float CeilingY = 5.1f;
    public const float GapCenterMin = -2.0f;
    public const float GapCenterMax = 2.8f;

    // ---- Palette ----
    public static readonly Color SepiaGold = new Color(0.91f, 0.71f, 0.30f);
    public static readonly Color Cream = new Color(0.96f, 0.90f, 0.78f);
    public static readonly Color DarkBrown = new Color(0.16f, 0.10f, 0.05f);
    public static readonly Color MarqueeRed = new Color(0.45f, 0.12f, 0.10f);
    public static readonly Color NightSky = new Color(0.023f, 0.039f, 0.094f);

    // ---- PlayerPrefs keys ----
    public const string HighScoreKey = "RiverReelHighScore";
    public const string CharacterKey = "RiverReelCharacter";

    public static float ScrollSpeedForScore(int score)
    {
        return Mathf.Min(BaseScrollSpeed + score * SpeedPerScore, MaxScrollSpeed);
    }

    public static float GapForScore(int score)
    {
        return Mathf.Max(BaseGap - score * GapPerScore, MinGap);
    }
}

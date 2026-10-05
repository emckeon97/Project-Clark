using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedural fallback art. Every texture the game needs is generated here
/// in code, so the project RUNS with zero binary assets. When Elijah drops
/// the real PNGs / MP3 into the project, the asset-wiring editor script
/// picks them up and this factory is only a safety net.
/// </summary>
public static class PlaceholderArt
{
    static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Texture2D> TextureCache = new Dictionary<string, Texture2D>();

    const float PPU = 100f;

    // ------------------------------------------------------------------
    // low-level drawing helpers (alpha-over compositing)
    // ------------------------------------------------------------------

    static void Blend(Texture2D t, int x, int y, Color c)
    {
        if (x < 0 || y < 0 || x >= t.width || y >= t.height || c.a <= 0f) return;
        Color dst = t.GetPixel(x, y);
        float a = c.a + dst.a * (1f - c.a);
        if (a <= 0f) { t.SetPixel(x, y, Color.clear); return; }
        Color outc = new Color(
            (c.r * c.a + dst.r * dst.a * (1f - c.a)) / a,
            (c.g * c.a + dst.g * dst.a * (1f - c.a)) / a,
            (c.b * c.a + dst.b * dst.a * (1f - c.a)) / a, a);
        t.SetPixel(x, y, outc);
    }

    static void FillRect(Texture2D t, int x0, int y0, int w, int h, Color c)
    {
        for (int y = y0; y < y0 + h; y++)
            for (int x = x0; x < x0 + w; x++)
                Blend(t, x, y, c);
    }

    static void FillEllipse(Texture2D t, float cx, float cy, float rx, float ry, Color c)
    {
        int x0 = Mathf.FloorToInt(cx - rx - 1), x1 = Mathf.CeilToInt(cx + rx + 1);
        int y0 = Mathf.FloorToInt(cy - ry - 1), y1 = Mathf.CeilToInt(cy + ry + 1);
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                float dx = (x - cx) / rx, dy = (y - cy) / ry;
                float d = dx * dx + dy * dy;
                if (d > 1f) continue;
                // soft 1px edge
                Color cc = c;
                if (d > 0.86f) cc.a *= Mathf.Clamp01((1f - d) / 0.14f);
                Blend(t, x, y, cc);
            }
    }

    static void FillCircle(Texture2D t, float cx, float cy, float r, Color c)
    {
        FillEllipse(t, cx, cy, r, r, c);
    }

    static void FillTriangle(Texture2D t, float x0, float y0, float x1, float y1, float x2, float y2, Color c)
    {
        int minX = Mathf.FloorToInt(Mathf.Min(x0, Mathf.Min(x1, x2)));
        int maxX = Mathf.CeilToInt(Mathf.Max(x0, Mathf.Max(x1, x2)));
        int minY = Mathf.FloorToInt(Mathf.Min(y0, Mathf.Min(y1, y2)));
        int maxY = Mathf.CeilToInt(Mathf.Max(y0, Mathf.Max(y1, y2)));
        float denom = (y1 - y2) * (x0 - x2) + (x2 - x1) * (y0 - y2);
        if (Mathf.Abs(denom) < 0.0001f) return;
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                float w1 = ((y1 - y2) * (x - x2) + (x2 - x1) * (y - y2)) / denom;
                float w2 = ((y2 - y0) * (x - x2) + (x0 - x2) * (y - y2)) / denom;
                float w3 = 1f - w1 - w2;
                if (w1 >= 0f && w2 >= 0f && w3 >= 0f) Blend(t, x, y, c);
            }
    }

    static void ThickArc(Texture2D t, float cx, float cy, float r, float a0deg, float a1deg, float thick, Color c)
    {
        for (float a = a0deg; a <= a1deg; a += 2f)
        {
            float rad = a * Mathf.Deg2Rad;
            FillCircle(t, cx + Mathf.Cos(rad) * r, cy + Mathf.Sin(rad) * r, thick, c);
        }
    }

    static Texture2D NewTex(int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        var clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                t.SetPixel(x, y, clear);
        return t;
    }

    static Sprite ToSprite(Texture2D t, string key)
    {
        if (SpriteCache.TryGetValue(key, out var s)) return s;
        t.Apply();
        s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), PPU);
        SpriteCache[key] = s;
        return s;
    }

    // ------------------------------------------------------------------
    // generic shapes
    // ------------------------------------------------------------------

    public static Sprite White
    {
        get
        {
            if (!TextureCache.TryGetValue("white", out var t))
            {
                t = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                for (int y = 0; y < 4; y++)
                    for (int x = 0; x < 4; x++)
                        t.SetPixel(x, y, Color.white);
                t.Apply();
                TextureCache["white"] = t;
            }
            return ToSprite(t, "white");
        }
    }

    public static Sprite Circle
    {
        get
        {
            if (SpriteCache.TryGetValue("circle", out var s)) return s;
            var t = NewTex(64, 64);
            FillCircle(t, 32, 32, 30, Color.white);
            return ToSprite(t, "circle");
        }
    }

    public static Sprite RoundedRect
    {
        get
        {
            if (SpriteCache.TryGetValue("rrect", out var s)) return s;
            var t = NewTex(64, 64);
            FillRect(t, 10, 4, 44, 56, Color.white);
            FillRect(t, 4, 10, 56, 44, Color.white);
            FillCircle(t, 10, 10, 7, Color.white);
            FillCircle(t, 54, 10, 7, Color.white);
            FillCircle(t, 10, 54, 7, Color.white);
            FillCircle(t, 54, 54, 7, Color.white);
            return ToSprite(t, "rrect");
        }
    }

    public static Sprite Glow
    {
        get
        {
            if (SpriteCache.TryGetValue("glow", out var s)) return s;
            var t = NewTex(128, 128);
            for (int y = 0; y < 128; y++)
                for (int x = 0; x < 128; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(64, 64)) / 64f;
                    if (d > 1f) continue;
                    float a = Mathf.Pow(1f - d, 2.2f);
                    Blend(t, x, y, new Color(1f, 1f, 1f, a));
                }
            return ToSprite(t, "glow");
        }
    }

    // ------------------------------------------------------------------
    // rubber-hose toon placeholder (front-facing face; charming at small size)
    // ------------------------------------------------------------------

    public static Sprite CharacterSprite(int index)
    {
        string key = "toon" + index;
        if (SpriteCache.TryGetValue(key, out var cached)) return cached;

        int n = GameConfig.CharacterFileNames.Length;
        index = ((index % n) + n) % n;
        Color body = GameConfig.CharacterBodyColors[index];
        Color accent = GameConfig.CharacterAccentColors[index];
        Color dark = new Color(0.08f, 0.06f, 0.06f);

        var t = NewTex(128, 128);

        // ears / headgear per character
        switch (index)
        {
            case 0: // felix - cat ears
                FillTriangle(t, 28, 92, 44, 122, 52, 90, body);
                FillTriangle(t, 100, 92, 84, 122, 76, 90, body);
                break;
            case 1: // popeye - sailor cap
                FillEllipse(t, 64, 108, 30, 10, Color.white);
                FillRect(t, 38, 104, 52, 14, Color.white);
                break;
            case 2: // oswald - long rabbit ears
                FillEllipse(t, 44, 116, 9, 22, body);
                FillEllipse(t, 84, 116, 9, 22, body);
                break;
            case 3: // koko - clown tufts
                FillCircle(t, 30, 100, 10, accent);
                FillCircle(t, 98, 100, 10, accent);
                break;
            case 4: // bimbo - floppy dog ears
                FillEllipse(t, 26, 88, 10, 22, accent);
                FillEllipse(t, 102, 88, 10, 22, accent);
                break;
            case 5: // pooh - round bear ears
                FillCircle(t, 32, 104, 12, body);
                FillCircle(t, 96, 104, 12, body);
                break;
            case 6: // olive - hair bun
                FillCircle(t, 64, 116, 12, dark);
                break;
            case 7: // bosko - derby hat
                FillEllipse(t, 64, 112, 26, 7, dark);
                FillRect(t, 44, 108, 40, 12, dark);
                break;
            case 8: // pete - bigger head, flat cap
                FillEllipse(t, 64, 110, 30, 8, dark);
                break;
        }

        // body
        FillEllipse(t, 64, 58, 38, 42, body);
        // belly patch
        Color belly = Color.Lerp(body, Color.white, 0.35f);
        FillEllipse(t, 64, 48, 22, 26, new Color(belly.r, belly.g, belly.b, 0.9f));

        // feet
        FillEllipse(t, 46, 14, 13, 9, dark);
        FillEllipse(t, 82, 14, 13, 9, dark);

        // eyes (pie-cut whites with outline)
        FillEllipse(t, 50, 76, 12, 14, dark);
        FillEllipse(t, 78, 76, 12, 14, dark);
        FillEllipse(t, 50, 76, 10, 12, Color.white);
        FillEllipse(t, 78, 76, 10, 12, Color.white);
        FillCircle(t, 52, 78, 4.5f, dark);
        FillCircle(t, 80, 78, 4.5f, dark);
        FillCircle(t, 53.5f, 79.5f, 1.5f, Color.white);
        FillCircle(t, 81.5f, 79.5f, 1.5f, Color.white);

        // angry brows for pete
        if (index == 8)
        {
            FillTriangle(t, 36, 92, 60, 84, 58, 90, dark);
            FillTriangle(t, 92, 92, 68, 84, 70, 90, dark);
        }

        // smile
        ThickArc(t, 64, 62, 16, 205f, 335f, 3f, dark);

        // koko's red nose over the smile
        if (index == 3) FillCircle(t, 64, 58, 6, accent);

        // blush
        Color blush = new Color(0.95f, 0.45f, 0.45f, 0.45f);
        FillCircle(t, 38, 62, 6, blush);
        FillCircle(t, 90, 62, 6, blush);

        return ToSprite(t, key);
    }

    // ------------------------------------------------------------------
    // smokestack (riveted steel, cap/lip drawn at the TOP of the texture;
    // the top stack uses flipY so its lip faces the gap)
    // ------------------------------------------------------------------

    public static Sprite Smokestack
    {
        get
        {
            if (SpriteCache.TryGetValue("stack", out var s)) return s;
            int W = 128, H = 512;
            var t = NewTex(W, H);
            Color top = new Color(0.42f, 0.24f, 0.15f);
            Color bot = new Color(0.20f, 0.11f, 0.08f);
            for (int y = 0; y < H; y++)
            {
                float k = y / (float)H;
                Color c = Color.Lerp(bot, top, k);
                for (int x = 0; x < W; x++) Blend(t, x, y, c);
            }
            // side shading: darker edges, warm center highlight
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float ex = Mathf.Abs(x - W / 2f) / (W / 2f);
                    float shade = 1f - ex * ex * 0.45f;
                    Color p = t.GetPixel(x, y);
                    t.SetPixel(x, y, new Color(p.r * shade + 0.05f * (1f - ex), p.g * shade, p.b * shade, 1f));
                }
            // plate bands
            for (int y = 48; y < H; y += 64)
            {
                for (int x = 0; x < W; x++)
                {
                    Blend(t, x, y, new Color(0, 0, 0, 0.35f));
                    if (y + 1 < H) Blend(t, x, y + 1, new Color(1f, 0.85f, 0.6f, 0.12f));
                }
            }
            // rivets: two columns
            for (int y = 70; y < H; y += 64)
                foreach (int x in new[] { 22, 106 })
                {
                    FillCircle(t, x, y, 5, new Color(0.12f, 0.07f, 0.05f));
                    FillCircle(t, x - 1, y + 1, 2, new Color(1f, 0.8f, 0.55f, 0.5f));
                }
            // cap lip at top (the gap-facing end)
            FillRect(t, -8, H - 44, W + 16, 44, new Color(0.30f, 0.16f, 0.10f));
            FillRect(t, -8, H - 44, W + 16, 6, new Color(0.55f, 0.34f, 0.20f));
            for (int x = 8; x < W + 8; x += 24)
                FillCircle(t, x, H - 22, 5, new Color(0.12f, 0.07f, 0.05f));
            return ToSprite(t, "stack");
        }
    }

    // ------------------------------------------------------------------
    // background layers
    // ------------------------------------------------------------------

    public static Sprite Sky
    {
        get
        {
            if (SpriteCache.TryGetValue("sky", out var s)) return s;
            var t = new Texture2D(8, 512, TextureFormat.RGBA32, false);
            Color top = new Color(0.012f, 0.020f, 0.055f);
            Color mid = new Color(0.05f, 0.08f, 0.18f);
            Color hor = new Color(0.13f, 0.16f, 0.30f);
            for (int y = 0; y < 512; y++)
            {
                float k = y / 511f;
                Color c = k < 0.6f ? Color.Lerp(top, mid, k / 0.6f) : Color.Lerp(mid, hor, (k - 0.6f) / 0.4f);
                for (int x = 0; x < 8; x++) t.SetPixel(x, y, c);
            }
            t.Apply();
            return ToSprite(t, "sky");
        }
    }

    public static Sprite Stars
    {
        get
        {
            if (SpriteCache.TryGetValue("stars", out var s)) return s;
            var t = NewTex(1024, 512);
            var rng = new System.Random(1930);
            for (int i = 0; i < 150; i++)
            {
                float x = (float)rng.NextDouble() * 1024f;
                float y = (float)rng.NextDouble() * 512f;
                float a = 0.25f + (float)rng.NextDouble() * 0.6f;
                float r = rng.NextDouble() < 0.12 ? 2.2f : 1.2f;
                FillCircle(t, x, y, r, new Color(1f, 1f, 1f, a));
            }
            return ToSprite(t, "stars");
        }
    }

    public static Sprite Moon
    {
        get
        {
            if (SpriteCache.TryGetValue("moon", out var s)) return s;
            var t = NewTex(256, 256);
            FillCircle(t, 128, 128, 92, new Color(0.96f, 0.94f, 0.82f));
            var rng = new System.Random(77);
            for (int i = 0; i < 9; i++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f;
                float d = (float)rng.NextDouble() * 55f;
                float r = 6f + (float)rng.NextDouble() * 14f;
                FillCircle(t, 128 + Mathf.Cos(a) * d, 128 + Mathf.Sin(a) * d, r,
                    new Color(0.82f, 0.80f, 0.68f, 0.8f));
            }
            return ToSprite(t, "moon");
        }
    }

    public static Sprite Cloud(int seed)
    {
        string key = "cloud" + seed;
        if (SpriteCache.TryGetValue(key, out var s)) return s;
        var t = NewTex(256, 128);
        var rng = new System.Random(seed);
        Color c = new Color(0.75f, 0.78f, 0.88f, 0.5f);
        for (int i = 0; i < 7; i++)
        {
            float x = 40 + (float)rng.NextDouble() * 176f;
            float y = 45 + (float)rng.NextDouble() * 40f;
            float rx = 28 + (float)rng.NextDouble() * 30f;
            float ry = 14 + (float)rng.NextDouble() * 14f;
            FillEllipse(t, x, y, rx, ry, c);
        }
        return ToSprite(t, key);
    }

    public static Sprite Skyline
    {
        get
        {
            if (SpriteCache.TryGetValue("skyline", out var s)) return s;
            int W = 1024, H = 256;
            var t = NewTex(W, H);
            var rng = new System.Random(4242);
            Color sil = new Color(0.03f, 0.045f, 0.09f);
            int x = 0;
            while (x < W)
            {
                int bw = 50 + rng.Next(90);
                int bh = 60 + rng.Next(150);
                FillRect(t, x, 0, bw, bh, sil);
                // smokestack on some buildings
                if (rng.NextDouble() < 0.4)
                {
                    int sx = x + bw / 2 - 8;
                    FillRect(t, sx, bh, 16, 50, sil);
                    FillRect(t, sx - 3, bh + 46, 22, 8, sil);
                }
                // lit windows
                for (int wy = 12; wy < bh - 8; wy += 18)
                    for (int wx = x + 8; wx < x + bw - 8; wx += 16)
                        if (rng.NextDouble() < 0.28)
                            FillRect(t, wx, wy, 6, 9, new Color(1f, 0.8f, 0.45f, 0.85f));
                x += bw + rng.Next(30);
            }
            return ToSprite(t, "skyline");
        }
    }

    public static Sprite Water
    {
        get
        {
            if (SpriteCache.TryGetValue("water", out var s)) return s;
            int W = 1024, H = 256;
            var t = new Texture2D(W, H, TextureFormat.RGBA32, false);
            Color top = new Color(0.07f, 0.13f, 0.24f);
            Color bot = new Color(0.015f, 0.03f, 0.07f);
            for (int y = 0; y < H; y++)
            {
                Color c = Color.Lerp(bot, top, y / (float)(H - 1));
                for (int xx = 0; xx < W; xx++) t.SetPixel(xx, y, c);
            }
            t.wrapMode = TextureWrapMode.Repeat;
            t.Apply();
            return ToSprite(t, "water");
        }
    }

    public static Sprite Shimmer
    {
        get
        {
            if (SpriteCache.TryGetValue("shimmer", out var s)) return s;
            int W = 512, H = 64;
            var t = NewTex(W, H);
            var rng = new System.Random(99);
            for (int i = 0; i < 60; i++)
            {
                int y = rng.Next(H);
                int x = rng.Next(W);
                int w = 20 + rng.Next(70);
                float a = 0.10f + (float)rng.NextDouble() * 0.25f;
                for (int xx = x; xx < x + w && xx < W; xx++)
                    Blend(t, xx, y, new Color(0.95f, 0.85f, 0.60f, a * (1f - Mathf.Abs(xx - x - w / 2f) / (w / 2f))));
            }
            t.wrapMode = TextureWrapMode.Repeat;
            return ToSprite(t, "shimmer");
        }
    }

    public static Sprite Vignette
    {
        get
        {
            if (SpriteCache.TryGetValue("vignette", out var s)) return s;
            int S = 256;
            var t = NewTex(S, S);
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(S / 2f, S / 2f)) / (S / 2f);
                    float a = Mathf.Clamp01((d - 0.55f) / 0.45f);
                    a = a * a * 0.62f;
                    if (a > 0.003f) Blend(t, x, y, new Color(0f, 0f, 0f, a));
                }
            return ToSprite(t, "vignette");
        }
    }

    public static Texture2D Grain
    {
        get
        {
            if (TextureCache.TryGetValue("grain", out var t)) return t;
            int S = 128;
            t = new Texture2D(S, S, TextureFormat.RGBA32, false);
            var rng = new System.Random(1234);
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float v = (float)rng.NextDouble();
                    t.SetPixel(x, y, new Color(v, v, v, 1f));
                }
            t.wrapMode = TextureWrapMode.Repeat;
            t.Apply();
            TextureCache["grain"] = t;
            return t;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moonlit river backdrop: gradient sky, stars, glowing moon, two cloud
/// layers, distant 1930s skyline with smokestacks, animated water with
/// moonlight shimmer and a moon reflection. All art is procedural.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    class Layer
    {
        public readonly List<SpriteRenderer> copies = new List<SpriteRenderer>();
        public float width;
        public float factor;

        public void Tick(float dt, float speed, float leftBound)
        {
            float dx = speed * factor * dt;
            foreach (var r in copies)
            {
                var p = r.transform.position;
                p.x -= dx;
                r.transform.position = p;
            }
            float total = width * copies.Count;
            foreach (var r in copies)
            {
                if (r.transform.position.x + width / 2f < leftBound)
                {
                    var p = r.transform.position;
                    p.x += total;
                    r.transform.position = p;
                }
            }
        }
    }

    readonly List<Layer> layers = new List<Layer>();
    SpriteRenderer shimmerA, shimmerB, moonGlow, moonReflection;
    float viewW;
    float time;

    static Texture2D MirrorTile(Texture2D src)
    {
        int w = src.width, h = src.height;
        var t = new Texture2D(w * 2, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = src.GetPixel(x, y);
                t.SetPixel(x, y, c);
                t.SetPixel(w * 2 - 1 - x, y, c);
            }
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        t.Apply();
        return t;
    }

    static Sprite TiledSprite(Sprite src)
    {
        var t = MirrorTile(src.texture);
        return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f);
    }

    SpriteRenderer AddSprite(string name, Sprite sprite, int order, Vector3 pos, Vector3 scale)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return sr;
    }

    Layer AddLayer(string name, Sprite tiled, int order, float y, float scaleXY, float factor, Color color)
    {
        var layer = new Layer { factor = factor };
        layer.width = (tiled.rect.width / tiled.pixelsPerUnit) * scaleXY;
        float coverage = viewW + 8f;
        int count = Mathf.CeilToInt(coverage / layer.width) + 1;
        float startX = -coverage / 2f;
        for (int i = 0; i < count; i++)
        {
            var sr = AddSprite(name + i, tiled, order,
                new Vector3(startX + layer.width / 2f + i * layer.width, y, 0f),
                new Vector3(scaleXY, scaleXY, 1f));
            sr.color = color;
            layer.copies.Add(sr);
        }
        layers.Add(layer);
        return layer;
    }

    public void Build(Camera cam)
    {
        float ortho = cam.orthographicSize;
        viewW = 2f * ortho * cam.aspect;
        float waterY = GameConfig.WaterY;

        // sky gradient fills the whole view
        var sky = AddSprite("Sky", PlaceholderArt.Sky, -10, new Vector3(0, 0, 0), Vector3.one);
        sky.transform.localScale = new Vector3((viewW + 4f) / (8f / 100f), (2f * ortho + 2f) / (512f / 100f), 1f);

        // stars (upper sky only)
        var stars = AddSprite("Stars", PlaceholderArt.Stars, -9, new Vector3(0, 1.6f, 0), Vector3.one);
        stars.transform.localScale = new Vector3((viewW + 4f) / (1024f / 100f), 1.4f, 1f);

        // moon + glow
        moonGlow = AddSprite("MoonGlow", PlaceholderArt.Glow, -8, new Vector3(2.6f, 2.7f, 0f), new Vector3(3.2f, 3.2f, 1f));
        moonGlow.color = new Color(0.95f, 0.92f, 0.75f, 0.35f);
        AddSprite("Moon", PlaceholderArt.Moon, -7, new Vector3(2.6f, 2.7f, 0f), new Vector3(1.05f, 1.05f, 1f));

        // clouds (two depths)
        AddLayer("CloudBack", TiledSprite(PlaceholderArt.Cloud(7)), -6, 3.1f, 1.7f, 0.10f,
            new Color(1f, 1f, 1f, 0.55f));
        AddLayer("CloudFront", TiledSprite(PlaceholderArt.Cloud(21)), -4, 1.5f, 2.4f, 0.22f,
            new Color(1f, 1f, 1f, 0.75f));

        // distant skyline silhouette, bottom edge at the waterline
        var skylineTiled = TiledSprite(PlaceholderArt.Skyline);
        float skyH = (skylineTiled.rect.height / skylineTiled.pixelsPerUnit);
        AddLayer("Skyline", skylineTiled, -5, waterY + skyH / 2f - 0.05f, 1f, 0.45f, Color.white);

        // water
        var waterTiled = TiledSprite(PlaceholderArt.Water);
        float waterWorldH = 2.0f;
        var waterLayer = AddLayer("Water", waterTiled, -3, waterY - waterWorldH / 2f + 0.02f, 1f, 0.55f, Color.white);
        foreach (var c in waterLayer.copies)
            c.transform.localScale = new Vector3(1f, waterWorldH / (waterTiled.rect.height / waterTiled.pixelsPerUnit), 1f);

        // moonlight shimmer strips on the water
        var shimmerTiled = TiledSprite(PlaceholderArt.Shimmer);
        var sh = AddLayer("Shimmer", shimmerTiled, -2, waterY - 0.35f, 1f, 0.75f, Color.white);
        shimmerA = sh.copies[0];
        var sh2 = AddLayer("Shimmer2", shimmerTiled, -2, waterY - 0.85f, 0.8f, 0.95f, Color.white);
        shimmerB = sh2.copies[0];

        // moon reflection streak
        moonReflection = AddSprite("MoonReflection", PlaceholderArt.Glow, -2,
            new Vector3(2.6f, waterY - 0.7f, 0f), new Vector3(0.55f, 1.9f, 1f));
        moonReflection.color = new Color(0.95f, 0.90f, 0.70f, 0.30f);
    }

    public void Tick(float dt, float speed, bool ambient)
    {
        time += dt;
        float leftBound = -viewW / 2f - 4f;
        foreach (var l in layers)
            l.Tick(dt, ambient ? Mathf.Max(speed, 0.35f) : speed, leftBound);

        // living details
        float pulse = 0.5f + 0.5f * Mathf.Sin(time * 1.7f);
        if (moonGlow != null)
            moonGlow.color = new Color(0.95f, 0.92f, 0.75f, 0.30f + 0.08f * pulse);
        if (moonReflection != null)
            moonReflection.color = new Color(0.95f, 0.90f, 0.70f, 0.22f + 0.12f * pulse);
        if (shimmerA != null)
            shimmerA.color = new Color(1f, 1f, 1f, 0.55f + 0.30f * Mathf.Sin(time * 2.3f));
        if (shimmerB != null)
            shimmerB.color = new Color(1f, 1f, 1f, 0.45f + 0.30f * Mathf.Sin(time * 2.9f + 1.3f));
    }
}

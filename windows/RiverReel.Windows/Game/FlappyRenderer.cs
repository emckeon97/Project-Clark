using SkiaSharp;
using RiverReel.Windows.Theme;

namespace RiverReel.Windows.Game;

/// <summary>
/// Night-flight rendering on a SkiaSharp canvas: moonlit sky, drifting
/// riverboat, riveted smokestack pairs, the flapping toon, pier deck,
/// score, get-ready card, film vignette.
/// Direct port of the iOS FlappyRenderer.
/// </summary>
public static class FlappyRenderer
{
    /// Deterministic pseudo-random in 0...1 (stable starfield).
    private static double Hash(int i)
    {
        double x = Math.Sin(i * 12.9898) * 43758.5453;
        return x - Math.Floor(x);
    }

    public static void Draw(SKCanvas canvas, FlappyEngine engine, SKBitmap? sprite, int width, int height)
    {
        float w = width, h = height;
        float groundY = (float)engine.GroundY;
        double tSec = engine.TSec;

        // ---- night sky ----
        using (var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0), new SKPoint(0, groundY),
                new[] { new SKColor(0x05, 0x07, 0x0F), new SKColor(0x0D, 0x13, 0x30) },
                null, SKShaderTileMode.Clamp)
        })
            canvas.DrawRect(0, 0, w, groundY, paint);

        // ---- stars ----
        for (int i = 0; i < 42; i++)
        {
            float sx = (float)(Hash(i * 3 + 1) * w);
            float sy = (float)(Hash(i * 3 + 2) * groundY * 0.85);
            float sr = (float)(0.8 + Hash(i * 3 + 3) * 1.4);
            double tw = 0.35 + 0.65 * Math.Abs(Math.Sin(tSec * (1 + Hash(i * 7) * 2) + i));
            using var paint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, (byte)(255 * 0.7 * tw)),
                IsAntialias = true,
            };
            canvas.DrawCircle(sx, sy, sr, paint);
        }

        // ---- moon + halo ----
        float moonX = w * 0.80f, moonY = h * 0.13f;
        float moonR = h * 0.042f;
        var cream = ClarkTheme.Cream;
        using (var halo1 = new SKPaint { Color = cream.WithAlpha((byte)(255 * 0.10)), IsAntialias = true })
            canvas.DrawCircle(moonX, moonY, moonR * 2.6f, halo1);
        using (var halo2 = new SKPaint { Color = cream.WithAlpha((byte)(255 * 0.16)), IsAntialias = true })
            canvas.DrawCircle(moonX, moonY, moonR * 1.7f, halo2);
        using (var moon = new SKPaint { Color = new SKColor(0xF2, 0xEB, 0xD8), IsAntialias = true })
            canvas.DrawCircle(moonX, moonY, moonR, moon);

        // ---- distant riverboat silhouette, drifting ----
        float boatW = 150;
        float boatX = (float)(w - (tSec * 14 % (w + boatW * 2)) - boatW);
        float boatY = groundY - 46;
        var hull = new SKColor(0x0B, 0x0E, 0x18);
        using (var hullPaint = new SKPaint { Color = hull, IsAntialias = true })
        {
            canvas.DrawRoundRect(new SKRect(boatX, boatY, boatX + boatW, boatY + 26), 6, 6, hullPaint);
            canvas.DrawRect(new SKRect(boatX + boatW * 0.3f, boatY - 20, boatX + boatW * 0.7f, boatY), hullPaint);
        }
        using (var winPaint = new SKPaint { Color = ClarkTheme.Gold.WithAlpha((byte)(255 * 0.75)), IsAntialias = true })
        {
            for (int i = 0; i < 4; i++)
            {
                float wx = boatX + boatW * (0.18f + i * 0.21f);
                canvas.DrawCircle(wx, boatY + 10, 3, winPaint);
            }
        }

        // ---- smokestack pairs ----
        foreach (var s in engine.Stacks)
        {
            float sx = (float)s.X;
            float sw = (float)FlappyEngine.PipeW;
            float gapTop = (float)(s.GapY - s.Gap / 2);
            float gapBot = (float)(s.GapY + s.Gap / 2);
            DrawStack(canvas, sx, sw, 0, gapTop, isTop: true);
            DrawStack(canvas, sx, sw, gapBot, groundY, isTop: false);
            // Lantern glow marking the gap.
            float lampX = sx + sw / 2;
            foreach (var (ly, alpha) in new[] { (gapTop - 4, 0.28), (gapBot + 4, 0.28) })
            {
                using var glow = new SKPaint { Color = ClarkTheme.Gold.WithAlpha((byte)(255 * alpha)), IsAntialias = true };
                canvas.DrawCircle(lampX, ly, 16, glow);
                using var lamp = new SKPaint { Color = new SKColor(0xE8, 0xC2, 0x5A), IsAntialias = true };
                canvas.DrawCircle(lampX, ly, 5, lamp);
            }
        }

        // ---- pier deck (the ground) ----
        using (var deck = new SKPaint { Color = new SKColor(0x24, 0x1C, 0x12) })
            canvas.DrawRect(0, groundY, w, h - groundY, deck);
        using (var deckTop = new SKPaint { Color = new SKColor(0x2E, 0x25, 0x17) })
            canvas.DrawRect(0, groundY, w, 10, deckTop);
        using (var edge = new SKPaint { Color = ClarkTheme.Gold.WithAlpha((byte)(255 * 0.5)), StrokeWidth = 2 })
            canvas.DrawLine(0, groundY + 1, w, groundY + 1, edge);
        // Plank seams.
        double seamMod = engine.Stacks.Count > 0 ? engine.Stacks[0].X % 48 : 0;
        using (var seam = new SKPaint { Color = new SKColor(0, 0, 0, (byte)(255 * 0.35)), StrokeWidth = 2 })
        {
            for (double sxp = -seamMod; sxp < w; sxp += 48)
                canvas.DrawLine((float)sxp, groundY + 10, (float)sxp, h, seam);
        }

        // ---- the toon ----
        float bx = (float)engine.BirdX, by = (float)engine.BirdY;
        const float birdPx = 64;
        double pulse = 1 + 0.13 * Math.Exp(-engine.FlapT * 7);
        canvas.Save();
        canvas.Translate(bx, by);
        canvas.RotateDegrees((float)engine.Rotation);
        canvas.Scale((float)pulse, (float)pulse);
        if (sprite != null)
        {
            float dh = birdPx;
            float dw = birdPx * sprite.Width / (float)sprite.Height;
            canvas.DrawBitmap(sprite, new SKRect(-dw / 2, -dh / 2, dw / 2, dh / 2));
        }
        else
        {
            using var body = new SKPaint { Color = new SKColor(0xF2, 0xEB, 0xD8), IsAntialias = true };
            canvas.DrawCircle(0, 0, birdPx * 0.42f, body);
            using var eye = new SKPaint { Color = SKColors.Black };
            canvas.DrawCircle(4, -10, 4, eye);
        }
        canvas.Restore();

        // ---- score ----
        using var scorePaint = new SKPaint
        {
            Typeface = SKTypeface.FromFamilyName("Georgia", SKFontStyleWeight.Black, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
            TextSize = 64,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
        };
        scorePaint.Color = new SKColor(0, 0, 0, (byte)(255 * 0.6));
        canvas.DrawText(engine.Score.ToString(), w / 2 + 3, 113 + 64, scorePaint);
        scorePaint.Color = cream;
        canvas.DrawText(engine.Score.ToString(), w / 2, 110 + 64, scorePaint);

        // ---- get ready ----
        if (!engine.Started && !engine.GameOver)
        {
            double blink = 0.65 + 0.35 * Math.Sin(tSec * 5);
            using var readyPaint = new SKPaint
            {
                Typeface = scorePaint.Typeface,
                TextSize = 30,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Color = ClarkTheme.Gold.WithAlpha((byte)(255 * blink)),
            };
            // Letter-spacing is approximated; SkiaSharp has no tracking API on DrawText.
            canvas.DrawText("GET READY!", w / 2, h * 0.30f + 30, readyPaint);
            using var tapPaint = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Georgia", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
                TextSize = 18,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Color = cream.WithAlpha((byte)(255 * 0.85 * blink)),
            };
            canvas.DrawText("TAP TO FLAP", w / 2, h * 0.30f + 52 + 18, tapPaint);
        }

        // ---- film vignette ----
        using (var vig = new SKPaint
        {
            Shader = SKShader.CreateRadialGradient(
                new SKPoint(w / 2, h / 2), Math.Max(w, h) * 0.62f,
                new[] { SKColors.Transparent, new SKColor(0, 0, 0, (byte)(255 * 0.38)) },
                null, SKShaderTileMode.Clamp)
        })
            canvas.DrawRect(0, 0, w, h, vig);
    }

    /// One riveted iron smokestack slab, with a cap lip on the gap end.
    private static void DrawStack(SKCanvas canvas, float x, float w, float top, float bottom, bool isTop)
    {
        if (bottom <= top) return;
        var iron = new SKColor(0x1A, 0x1D, 0x24);
        using (var p = new SKPaint { Color = iron })
            canvas.DrawRect(x, top, w, bottom - top, p);
        // Vertical highlight.
        using (var hl = new SKPaint { Color = new SKColor(0x2E, 0x34, 0x40, (byte)(255 * 0.8)) })
            canvas.DrawRect(x + w * 0.14f, top, w * 0.12f, bottom - top, hl);
        // Gold bands + rivets.
        float y = top + 26;
        using var band = new SKPaint { Color = ClarkTheme.Gold.WithAlpha((byte)(255 * 0.85)) };
        using var rivet = new SKPaint { Color = new SKColor(0x0B, 0x0D, 0x12), IsAntialias = true };
        while (y < bottom - 10)
        {
            canvas.DrawRect(x, y, w, 7, band);
            for (float rx = x + 10; rx < x + w - 6; rx += 18)
                canvas.DrawCircle(rx, y + 3.5f, 2.6f, rivet);
            y += 64;
        }
        // Cap lip on the gap end.
        const float lipH = 15;
        float lipY = isTop ? bottom - lipH : top;
        using (var lip = new SKPaint { Color = new SKColor(0x0B, 0x0D, 0x12) })
            canvas.DrawRect(x - 7, lipY, w + 14, lipH, lip);
        using (var lipGold = new SKPaint { Color = ClarkTheme.Gold.WithAlpha((byte)(255 * 0.7)) })
            canvas.DrawRect(x - 7, isTop ? lipY : lipY + lipH - 3, w + 14, 3, lipGold);
    }
}

using System.Collections;
using UnityEngine;

/// <summary>
/// The toon. Transform-based physics (no Rigidbody — deterministic and
/// immune to physics-config pitfalls): gravity, flap impulse, tilt, and
/// squash-and-stretch. Collision is manual AABB, checked by GameManager.
/// </summary>
public class Player : MonoBehaviour
{
    GameManager gm;
    SpriteRenderer sr;
    Coroutine punchRoutine;
    float vy;
    float bobTime;
    bool menuBobbing;
    float baseScale = 1f;
    bool splashed;

    public bool Alive { get; private set; }
    public bool DoneSinking { get; private set; }

    public Vector2 Min => (Vector2)transform.position - Vector2.one * GameConfig.PlayerHalfSize;
    public Vector2 Max => (Vector2)transform.position + Vector2.one * GameConfig.PlayerHalfSize;

    public void Init(GameManager manager)
    {
        gm = manager;
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        ResetPlayer();
    }

    public void SetCharacter(int index)
    {
        Sprite sprite = null;
        var lib = Resources.Load<CharacterLibrary>("CharacterLibrary");
        if (lib != null) sprite = lib.Get(index);
        if (sprite == null) sprite = PlaceholderArt.CharacterSprite(index);
        sr.sprite = sprite;
        // normalize visual size: real PNGs and placeholders differ in pixels
        float targetH = 0.95f;
        if (sprite != null && sprite.rect.height > 0f)
        {
            float worldH = sprite.rect.height / sprite.pixelsPerUnit;
            baseScale = targetH / worldH;
            transform.localScale = Vector3.one * baseScale;
        }
        else
        {
            baseScale = 1f;
        }
    }

    public void ResetPlayer()
    {
        transform.position = new Vector3(GameConfig.PlayerX, 0.8f, 0f);
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one * baseScale;
        vy = 0f;
        Alive = true;
        DoneSinking = false;
        splashed = false;
        menuBobbing = true;
        bobTime = 0f;
        if (punchRoutine != null) { StopCoroutine(punchRoutine); punchRoutine = null; }
    }

    public void BeginPlay()
    {
        menuBobbing = false;
    }

    public void MenuBob(float dt)
    {
        if (!menuBobbing) return;
        bobTime += dt;
        var p = transform.position;
        p.y = 0.8f + Mathf.Sin(bobTime * 2.2f) * 0.25f;
        transform.position = p;
    }

    public void Flap()
    {
        if (!Alive) return;
        vy = GameConfig.FlapVelocity;
        gm.Audio.PlayFlap();
        if (punchRoutine != null) StopCoroutine(punchRoutine);
        punchRoutine = StartCoroutine(Punch());
    }

    IEnumerator Punch()
    {
        float t = 0f;
        const float dur = 0.16f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            // squash then recover with a touch of overshoot
            float sx = Mathf.Lerp(0.78f, 1f, k);
            float sy = Mathf.Lerp(1.28f, 1f, k);
            if (k > 0.7f)
            {
                float o = (k - 0.7f) / 0.3f;
                sx = 1f + Mathf.Sin(o * Mathf.PI) * 0.06f;
                sy = 1f - Mathf.Sin(o * Mathf.PI) * 0.06f;
            }
            transform.localScale = new Vector3(sx * baseScale, sy * baseScale, 1f);
            yield return null;
        }
        transform.localScale = Vector3.one * baseScale;
        punchRoutine = null;
    }

    public void Tick(float dt)
    {
        if (!Alive) return;
        vy += GameConfig.Gravity * dt;
        if (vy < GameConfig.MaxFallSpeed) vy = GameConfig.MaxFallSpeed;
        var p = transform.position;
        p.y += vy * dt;
        if (p.y > GameConfig.CeilingY)
        {
            p.y = GameConfig.CeilingY;
            if (vy > 0f) vy = 0f;
        }
        transform.position = p;

        // tilt: nose up when rising, dive when falling
        float targetZ = Mathf.Clamp(-vy * 5.5f, -65f, 28f);
        float z = Mathf.LerpAngle(transform.eulerAngles.z, targetZ, dt * 10f);
        transform.eulerAngles = new Vector3(0f, 0f, z);

        if (p.y <= GameConfig.WaterY + 0.05f)
            Die(true);
    }

    public void TickDead(float dt)
    {
        if (Alive) return;
        vy += GameConfig.Gravity * dt;
        if (vy < GameConfig.MaxFallSpeed) vy = GameConfig.MaxFallSpeed;
        var p = transform.position;
        p.y += vy * dt;
        transform.position = p;
        if (!splashed && p.y <= GameConfig.WaterY + 0.05f)
        {
            splashed = true;
            gm.Particles.Burst(new Vector3(p.x, GameConfig.WaterY, 0f),
                new Color(0.65f, 0.80f, 0.95f), 22, 3.0f, 0.8f, -9f);
            gm.Audio.PlaySplash();
        }
        if (p.y < GameConfig.WaterY - 0.6f)
            DoneSinking = true; // resting under water; GameManager stops ticking
        float z = Mathf.LerpAngle(transform.eulerAngles.z, -80f, dt * 6f);
        transform.eulerAngles = new Vector3(0f, 0f, z);
    }

    public void Die(bool inWater)
    {
        if (!Alive) return;
        Alive = false;
        var pos = transform.position;
        if (inWater)
        {
            splashed = true;
            gm.Particles.Burst(pos, new Color(0.65f, 0.80f, 0.95f), 26, 3.2f, 0.8f, -9f);
            gm.Audio.PlaySplash();
        }
        else
        {
            gm.Particles.Burst(pos, new Color(0.95f, 0.85f, 0.60f), 18, 2.6f, 0.7f, -6f);
        }
        gm.OnPlayerDied();
    }
}

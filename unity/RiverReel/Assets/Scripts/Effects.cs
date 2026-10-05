using UnityEngine;

/// <summary>
/// Tiny pooled sprite particles (water splash, score sparkle, death poof).
/// No Unity particle system needed — plain SpriteRenderers.
/// </summary>
public class ParticlePool : MonoBehaviour
{
    struct P
    {
        public bool active;
        public Vector2 pos;
        public Vector2 vel;
        public float life;
        public float maxLife;
        public float gravity;
        public float size;
        public Color color;
    }

    P[] parts;
    SpriteRenderer[] renderers;
    Transform poolRoot;

    public void Init(int capacity)
    {
        parts = new P[capacity];
        renderers = new SpriteRenderer[capacity];
        poolRoot = new GameObject("Particles").transform;
        poolRoot.SetParent(transform, false);
        Sprite dot = PlaceholderArt.Circle;
        for (int i = 0; i < capacity; i++)
        {
            var go = new GameObject("p" + i);
            go.transform.SetParent(poolRoot, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = dot;
            sr.sortingOrder = 20;
            sr.enabled = false;
            renderers[i] = sr;
        }
    }

    public void Burst(Vector3 worldPos, Color color, int count, float speed, float life, float gravity)
    {
        int spawned = 0;
        for (int i = 0; i < parts.Length && spawned < count; i++)
        {
            if (parts[i].active) continue;
            float a = Random.Range(0f, Mathf.PI * 2f);
            float sp = Random.Range(speed * 0.4f, speed);
            parts[i].active = true;
            parts[i].pos = worldPos;
            parts[i].vel = new Vector2(Mathf.Cos(a) * sp, Mathf.Sin(a) * sp + speed * 0.35f);
            parts[i].life = parts[i].maxLife = life * Random.Range(0.7f, 1.2f);
            parts[i].gravity = gravity;
            parts[i].size = Random.Range(0.08f, 0.2f);
            parts[i].color = color;
            spawned++;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < parts.Length; i++)
        {
            if (!parts[i].active)
            {
                if (renderers[i].enabled) renderers[i].enabled = false;
                continue;
            }
            var p = parts[i];
            p.life -= dt;
            if (p.life <= 0f)
            {
                p.active = false;
                parts[i] = p;
                renderers[i].enabled = false;
                continue;
            }
            p.vel.y += p.gravity * dt;
            p.pos += p.vel * dt;
            parts[i] = p;
            var sr = renderers[i];
            sr.enabled = true;
            sr.transform.position = new Vector3(p.pos.x, p.pos.y, 0f);
            float k = p.life / p.maxLife;
            sr.color = new Color(p.color.r, p.color.g, p.color.b, p.color.a * k);
            sr.transform.localScale = new Vector3(p.size, p.size, 1f);
        }
    }
}

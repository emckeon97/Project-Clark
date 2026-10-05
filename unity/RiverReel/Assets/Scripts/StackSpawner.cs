using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns smokestack pairs at spacing-based intervals. Speed and gap
/// ramp with the score via GameConfig.
/// </summary>
public class StackSpawner : MonoBehaviour
{
    GameManager gm;
    readonly List<StackPair> pairs = new List<StackPair>();
    float spawnTimer;
    bool running;

    public float CurrentSpeed { get; private set; }
    public float CurrentGap { get; private set; }

    public void Init(GameManager manager)
    {
        gm = manager;
        CurrentSpeed = GameConfig.BaseScrollSpeed;
        CurrentGap = GameConfig.BaseGap;
    }

    public void Begin()
    {
        Clear();
        running = true;
        spawnTimer = 1.1f; // breather before the first pair
    }

    public void Stop()
    {
        running = false;
    }

    public void Clear()
    {
        foreach (var p in pairs)
            if (p != null) Object.Destroy(p.gameObject);
        pairs.Clear();
    }

    public void Tick(float dt)
    {
        CurrentSpeed = GameConfig.ScrollSpeedForScore(gm.Score);
        CurrentGap = GameConfig.GapForScore(gm.Score);

        if (running)
        {
            spawnTimer -= dt;
            if (spawnTimer <= 0f)
            {
                Spawn();
                spawnTimer = GameConfig.StackSpacing / CurrentSpeed;
            }
        }

        for (int i = pairs.Count - 1; i >= 0; i--)
        {
            var p = pairs[i];
            if (p == null) { pairs.RemoveAt(i); continue; }
            if (!p.Tick(dt, CurrentSpeed))
            {
                Object.Destroy(p.gameObject);
                pairs.RemoveAt(i);
            }
        }
    }

    void Spawn()
    {
        float center = Random.Range(GameConfig.GapCenterMin, GameConfig.GapCenterMax);
        var pair = StackPair.Create(transform, center, CurrentGap);
        pairs.Add(pair);
    }

    public bool CheckCollision(Vector2 pMin, Vector2 pMax)
    {
        foreach (var p in pairs)
            if (p != null && p.Overlaps(pMin, pMax))
                return true;
        return false;
    }

    public StackPair PassedPair(float playerX)
    {
        foreach (var p in pairs)
            if (p != null && !p.Scored && p.X + GameConfig.StackWidth / 2f < playerX)
                return p;
        return null;
    }
}

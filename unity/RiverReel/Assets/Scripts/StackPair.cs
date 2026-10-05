using UnityEngine;

/// <summary>
/// One smokestack pair: a top stack and a bottom stack with a gap between.
/// Moves left at the scroll speed; exposes AABBs for manual collision.
/// </summary>
public class StackPair : MonoBehaviour
{
    float gapTop;
    float gapBottom;
    float halfW;
    public bool Scored { get; set; }

    public float X => transform.position.x;

    public static StackPair Create(Transform parent, float gapCenterY, float gapSize)
    {
        var go = new GameObject("StackPair");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(GameConfig.SpawnX, 0f, 0f);
        var pair = go.AddComponent<StackPair>();

        float stackH = 7f; // tall enough to cover the screen above/below the gap
        Sprite stackSprite = PlaceholderArt.Smokestack;
        float spriteW = stackSprite.rect.width / stackSprite.pixelsPerUnit;
        float scaleX = GameConfig.StackWidth / spriteW;

        // bottom stack: cap lip at its TOP (gap side)
        var bottom = new GameObject("Bottom");
        bottom.transform.SetParent(go.transform, false);
        var bSr = bottom.AddComponent<SpriteRenderer>();
        bSr.sprite = stackSprite;
        bSr.sortingOrder = 0;
        bottom.transform.localScale = new Vector3(scaleX, stackH / (stackSprite.rect.height / stackSprite.pixelsPerUnit), 1f);

        // top stack: flipped so its cap lip faces DOWN toward the gap
        var top = new GameObject("Top");
        top.transform.SetParent(go.transform, false);
        var tSr = top.AddComponent<SpriteRenderer>();
        tSr.sprite = stackSprite;
        tSr.flipY = true;
        tSr.sortingOrder = 0;
        top.transform.localScale = bottom.transform.localScale;

        pair.gapTop = gapCenterY + gapSize / 2f;
        pair.gapBottom = gapCenterY - gapSize / 2f;
        pair.halfW = GameConfig.StackWidth / 2f * 0.92f; // slightly forgiving

        bottom.transform.position = new Vector3(GameConfig.SpawnX, pair.gapBottom - stackH / 2f, 0f);
        top.transform.position = new Vector3(GameConfig.SpawnX, pair.gapTop + stackH / 2f, 0f);
        return pair;
    }

    /// <summary>Move left; returns false when it should be despawned.</summary>
    public bool Tick(float dt, float speed)
    {
        var p = transform.position;
        p.x -= speed * dt;
        transform.position = p;
        return p.x > GameConfig.DespawnX;
    }

    public bool Overlaps(Vector2 pMin, Vector2 pMax)
    {
        float x0 = X - halfW, x1 = X + halfW;
        if (pMax.x < x0 || pMin.x > x1) return false;
        // top stack occupies [gapTop, +inf), bottom (-inf, gapBottom]
        if (pMax.y > gapTop && pMin.y < gapTop + 8f) return true;
        if (pMin.y < gapBottom && pMax.y > gapBottom - 8f) return true;
        return false;
    }
}

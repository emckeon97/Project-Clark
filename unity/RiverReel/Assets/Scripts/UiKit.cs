using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tiny helpers for building Unity UI in code: panels, text, buttons, images.
/// </summary>
public static class UiKit
{
    static Font _font;

    public static Font DefaultFont()
    {
        if (_font == null)
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return _font;
    }

    public static RectTransform Rect(GameObject go)
    {
        return go.GetComponent<RectTransform>();
    }

    public static void FullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    public static void Center(RectTransform rt, Vector2 size, Vector2 anchoredPos)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
    }

    public static Image MakeImage(Transform parent, string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    public static Text MakeText(Transform parent, string name, string content, int size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = DefaultFont();
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        return t;
    }

    public static Button MakeButton(Transform parent, string name, string label, Vector2 size,
        Vector2 anchoredPos, Color bg, Color labelColor, int labelSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        Center(rt, size, anchoredPos);
        var img = go.GetComponent<Image>();
        img.sprite = PlaceholderArt.RoundedRect;
        img.type = Image.Type.Sliced;
        img.color = bg;
        var t = MakeText(go.transform, "Label", label, labelSize, labelColor);
        FullStretch(t.rectTransform);
        return go.GetComponent<Button>();
    }

    public static void AddOutline(Text t, Color color)
    {
        var o = t.gameObject.AddComponent<Outline>();
        o.effectColor = color;
        o.effectDistance = new Vector2(2f, -2f);
    }
}

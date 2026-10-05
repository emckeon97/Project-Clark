using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// All UI is built procedurally: menu (marquee title, character select,
/// high score), HUD score, game-over panel, score popups, and the
/// film-grain/vignette overlay. No scene wiring required.
/// </summary>
public class UIManager : MonoBehaviour
{
    GameManager gm;
    Camera cam;

    Canvas canvas;
    RectTransform canvasRect;

    GameObject menuRoot, hudRoot, gameOverRoot, popupRoot;
    Text scoreText, highScoreText, charNameText, finalScoreText, finalBestText, newBestText;
    Image charPreview;
    List<Image> bulbs = new List<Image>();
    Text[] popups;
    Coroutine[] popupRoutines;
    int popupCursor;
    Image grainImage;
    float grainTimer;

    Button retryButton, menuButton, prevButton, nextButton;

    public void Build(GameManager manager)
    {
        gm = manager;
        cam = Camera.main;

        // Canvas
        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(transform, false);
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(540, 960);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasRect = canvasGo.GetComponent<RectTransform>();

        // EventSystem (required for buttons)
        var es = new GameObject("EventSystem");
        es.transform.SetParent(transform, false);
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        BuildMenu();
        BuildHud();
        BuildGameOver();
        BuildPopups();
        BuildFilmOverlay();

        ShowMenu();
    }

    // ------------------------------------------------------------------
    // menu
    // ------------------------------------------------------------------

    void BuildMenu()
    {
        menuRoot = new GameObject("Menu");
        menuRoot.transform.SetParent(canvas.transform, false);
        UiKit.FullStretch(menuRoot.AddComponent<RectTransform>());

        // marquee sign panel
        var sign = UiKit.MakeImage(menuRoot.transform, "Sign", PlaceholderArt.RoundedRect,
            new Color(0.30f, 0.09f, 0.07f, 0.96f));
        UiKit.Center(sign.rectTransform, new Vector2(470, 190), new Vector2(0, 300));
        var signBorder = UiKit.MakeImage(menuRoot.transform, "SignBorder", PlaceholderArt.RoundedRect,
            new Color(0.85f, 0.62f, 0.25f, 0.9f));
        UiKit.Center(signBorder.rectTransform, new Vector2(486, 206), new Vector2(0, 300));
        signBorder.transform.SetSiblingIndex(0);

        // marquee bulbs around the sign
        bulbs.Clear();
        int bulbCount = 16;
        for (int i = 0; i < bulbCount; i++)
        {
            bool topRow = i % 2 == 0;
            var b = UiKit.MakeImage(menuRoot.transform, "Bulb" + i, PlaceholderArt.Circle,
                GameConfig.SepiaGold);
            float x = Mathf.Lerp(-225f, 225f, (i / 2) / (float)(bulbCount / 2 - 1));
            float y = 300 + (topRow ? 108f : -108f);
            UiKit.Center(b.rectTransform, new Vector2(16, 16), new Vector2(x, y));
            bulbs.Add(b);
        }

        var title = UiKit.MakeText(menuRoot.transform, "Title", "RIVER REEL", 64, GameConfig.SepiaGold);
        UiKit.Center(title.rectTransform, new Vector2(460, 90), new Vector2(0, 322));
        UiKit.AddOutline(title, new Color(0.10f, 0.05f, 0.02f));

        var sub = UiKit.MakeText(menuRoot.transform, "Sub", "~ a 1930s river romp ~", 20, GameConfig.Cream);
        UiKit.Center(sub.rectTransform, new Vector2(460, 30), new Vector2(0, 252));

        highScoreText = UiKit.MakeText(menuRoot.transform, "High", "BEST  0", 26, GameConfig.Cream);
        UiKit.Center(highScoreText.rectTransform, new Vector2(400, 40), new Vector2(0, 170));

        // character select row
        prevButton = UiKit.MakeButton(menuRoot.transform, "Prev", "<", new Vector2(70, 70),
            new Vector2(-150, 20), new Color(0.35f, 0.12f, 0.10f, 0.95f), GameConfig.Cream, 34);
        prevButton.onClick.AddListener(() => { gm.Audio.PlayClick(); gm.SelectCharacter(gm.SelectedCharacter - 1); });

        nextButton = UiKit.MakeButton(menuRoot.transform, "Next", ">", new Vector2(70, 70),
            new Vector2(150, 20), new Color(0.35f, 0.12f, 0.10f, 0.95f), GameConfig.Cream, 34);
        nextButton.onClick.AddListener(() => { gm.Audio.PlayClick(); gm.SelectCharacter(gm.SelectedCharacter + 1); });

        charPreview = UiKit.MakeImage(menuRoot.transform, "CharPreview", PlaceholderArt.CharacterSprite(0), Color.white);
        UiKit.Center(charPreview.rectTransform, new Vector2(130, 130), new Vector2(0, 20));
        charPreview.preserveAspect = true;

        charNameText = UiKit.MakeText(menuRoot.transform, "CharName", "Felix", 26, GameConfig.SepiaGold);
        UiKit.Center(charNameText.rectTransform, new Vector2(400, 40), new Vector2(0, -70));

        var hint = UiKit.MakeText(menuRoot.transform, "Hint", "TAP TO START", 30, GameConfig.Cream);
        UiKit.Center(hint.rectTransform, new Vector2(400, 44), new Vector2(0, -230));
        StartCoroutine(Blink(hint));

        var credit = UiKit.MakeText(menuRoot.transform, "Credit",
            "Music: \"The Entertainer\" - Kevin MacLeod (CC-BY 4.0)", 13,
            new Color(0.96f, 0.90f, 0.78f, 0.55f));
        UiKit.Center(credit.rectTransform, new Vector2(520, 24), new Vector2(0, -452));
    }

    IEnumerator Blink(Text t)
    {
        while (t != null)
        {
            t.enabled = !t.enabled;
            yield return new WaitForSeconds(0.55f);
        }
    }

    // ------------------------------------------------------------------
    // HUD
    // ------------------------------------------------------------------

    void BuildHud()
    {
        hudRoot = new GameObject("HUD");
        hudRoot.transform.SetParent(canvas.transform, false);
        UiKit.FullStretch(hudRoot.AddComponent<RectTransform>());

        scoreText = UiKit.MakeText(hudRoot.transform, "Score", "0", 72, GameConfig.Cream);
        UiKit.Center(scoreText.rectTransform, new Vector2(300, 100), new Vector2(0, 390));
        UiKit.AddOutline(scoreText, new Color(0.08f, 0.05f, 0.03f));
    }

    // ------------------------------------------------------------------
    // game over
    // ------------------------------------------------------------------

    void BuildGameOver()
    {
        gameOverRoot = new GameObject("GameOver");
        gameOverRoot.transform.SetParent(canvas.transform, false);
        UiKit.FullStretch(gameOverRoot.AddComponent<RectTransform>());

        var dim = UiKit.MakeImage(gameOverRoot.transform, "Dim", PlaceholderArt.White, new Color(0, 0, 0, 0.55f));
        UiKit.FullStretch(dim.rectTransform);

        var panel = UiKit.MakeImage(gameOverRoot.transform, "Panel", PlaceholderArt.RoundedRect,
            new Color(0.30f, 0.09f, 0.07f, 0.97f));
        UiKit.Center(panel.rectTransform, new Vector2(420, 380), new Vector2(0, 40));

        var title = UiKit.MakeText(gameOverRoot.transform, "Title", "SUNK!", 58, GameConfig.SepiaGold);
        UiKit.Center(title.rectTransform, new Vector2(400, 80), new Vector2(0, 170));
        UiKit.AddOutline(title, new Color(0.10f, 0.05f, 0.02f));

        finalScoreText = UiKit.MakeText(gameOverRoot.transform, "Final", "SCORE  0", 36, GameConfig.Cream);
        UiKit.Center(finalScoreText.rectTransform, new Vector2(400, 50), new Vector2(0, 90));

        finalBestText = UiKit.MakeText(gameOverRoot.transform, "Best", "BEST  0", 26, GameConfig.Cream);
        UiKit.Center(finalBestText.rectTransform, new Vector2(400, 40), new Vector2(0, 40));

        newBestText = UiKit.MakeText(gameOverRoot.transform, "NewBest", "* NEW BEST! *", 26, GameConfig.SepiaGold);
        UiKit.Center(newBestText.rectTransform, new Vector2(400, 40), new Vector2(0, -8));

        retryButton = UiKit.MakeButton(gameOverRoot.transform, "Retry", "RETRY", new Vector2(220, 64),
            new Vector2(0, -90), new Color(0.55f, 0.16f, 0.11f, 1f), GameConfig.Cream, 30);
        retryButton.onClick.AddListener(() => { gm.Audio.PlayClick(); gm.StartGame(); });

        menuButton = UiKit.MakeButton(gameOverRoot.transform, "Menu", "MENU", new Vector2(220, 54),
            new Vector2(0, -165), new Color(0.25f, 0.18f, 0.12f, 1f), GameConfig.Cream, 24);
        menuButton.onClick.AddListener(() => { gm.Audio.PlayClick(); gm.ReturnToMenu(); });
    }

    // ------------------------------------------------------------------
    // score popups
    // ------------------------------------------------------------------

    void BuildPopups()
    {
        popupRoot = new GameObject("Popups");
        popupRoot.transform.SetParent(canvas.transform, false);
        UiKit.FullStretch(popupRoot.AddComponent<RectTransform>());
        popups = new Text[6];
        popupRoutines = new Coroutine[6];
        for (int i = 0; i < popups.Length; i++)
        {
            var t = UiKit.MakeText(popupRoot.transform, "Popup" + i, "+1", 30, GameConfig.SepiaGold);
            UiKit.AddOutline(t, new Color(0.08f, 0.05f, 0.02f));
            t.enabled = false;
            popups[i] = t;
        }
    }

    public void ScorePopup(Vector3 worldPos)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out local))
            return;
        var t = popups[popupCursor];
        int slot = popupCursor;
        popupCursor = (popupCursor + 1) % popups.Length;
        if (popupRoutines[slot] != null) StopCoroutine(popupRoutines[slot]);
        t.rectTransform.anchoredPosition = local;
        t.enabled = true;
        t.color = GameConfig.SepiaGold;
        popupRoutines[slot] = StartCoroutine(PopupAnim(t));
    }

    IEnumerator PopupAnim(Text t)
    {
        float dur = 0.7f, t0 = Time.time;
        Vector2 start = t.rectTransform.anchoredPosition;
        while (Time.time - t0 < dur)
        {
            float k = (Time.time - t0) / dur;
            t.rectTransform.anchoredPosition = start + new Vector2(0, k * 60f);
            t.color = new Color(GameConfig.SepiaGold.r, GameConfig.SepiaGold.g, GameConfig.SepiaGold.b, 1f - k);
            yield return null;
        }
        t.enabled = false;
    }

    // ------------------------------------------------------------------
    // film grain + vignette overlay
    // ------------------------------------------------------------------

    void BuildFilmOverlay()
    {
        var vig = UiKit.MakeImage(canvas.transform, "Vignette", PlaceholderArt.Vignette, Color.white);
        UiKit.FullStretch(vig.rectTransform);

        grainImage = UiKit.MakeImage(canvas.transform, "Grain", PlaceholderArt.White, new Color(1, 1, 1, 0.04f));
        UiKit.FullStretch(grainImage.rectTransform);
        grainImage.sprite = Sprite.Create(PlaceholderArt.Grain,
            new Rect(0, 0, PlaceholderArt.Grain.width, PlaceholderArt.Grain.height),
            new Vector2(0.5f, 0.5f), 32f);
        grainImage.type = Image.Type.Tiled;
    }

    void Update()
    {
        // marquee chase lights
        float t = Time.time * 6f;
        for (int i = 0; i < bulbs.Count; i++)
        {
            bool lit = ((int)(t + i) % 4) < 2;
            bulbs[i].color = lit ? new Color(1f, 0.85f, 0.45f) : new Color(0.45f, 0.30f, 0.12f);
        }
        // film grain jitter
        grainTimer -= Time.deltaTime;
        if (grainTimer <= 0f && grainImage != null)
        {
            grainTimer = 0.09f;
            grainImage.rectTransform.anchoredPosition = new Vector2(Random.Range(-6f, 6f), Random.Range(-6f, 6f));
            grainImage.color = new Color(1f, 1f, 1f, Random.Range(0.025f, 0.06f));
        }
    }

    // ------------------------------------------------------------------
    // public state API
    // ------------------------------------------------------------------

    public void ShowMenu()
    {
        menuRoot.SetActive(true);
        hudRoot.SetActive(false);
        gameOverRoot.SetActive(false);
        SetHighScore(gm.HighScore);
        RefreshCharacter(gm.SelectedCharacter);
    }

    public void ShowHUD()
    {
        menuRoot.SetActive(false);
        hudRoot.SetActive(true);
        gameOverRoot.SetActive(false);
        SetScore(0);
    }

    public void ShowGameOver(int score, int best, bool newBest)
    {
        menuRoot.SetActive(false);
        hudRoot.SetActive(false);
        gameOverRoot.SetActive(true);
        finalScoreText.text = "SCORE  " + score;
        finalBestText.text = "BEST  " + best;
        newBestText.enabled = newBest;
    }

    public void SetScore(int s)
    {
        if (scoreText != null) scoreText.text = s.ToString();
    }

    public void SetHighScore(int h)
    {
        if (highScoreText != null) highScoreText.text = "BEST  " + h;
    }

    public void RefreshCharacter(int index)
    {
        Sprite sprite = null;
        var lib = Resources.Load<CharacterLibrary>("CharacterLibrary");
        if (lib != null) sprite = lib.Get(index);
        if (sprite == null) sprite = PlaceholderArt.CharacterSprite(index);
        if (charPreview != null)
        {
            charPreview.sprite = sprite;
            charPreview.preserveAspect = true;
        }
        int n = GameConfig.CharacterDisplayNames.Length;
        int safe = ((index % n) + n) % n;
        if (charNameText != null) charNameText.text = GameConfig.CharacterDisplayNames[safe];
    }
}

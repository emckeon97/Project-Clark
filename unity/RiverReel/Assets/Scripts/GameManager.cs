using System.Collections;
using UnityEngine;

/// <summary>
/// Root of the game. Builds every subsystem procedurally in Awake,
/// owns the state machine (Menu / Playing / GameOver), input, scoring,
/// difficulty, and persistence.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public enum State { Menu, Playing, GameOver }
    public State CurrentState { get; private set; }

    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public int SelectedCharacter { get; private set; }

    public Player Player { get; private set; }
    public StackSpawner Spawner { get; private set; }
    public UIManager UI { get; private set; }
    public AudioManager Audio { get; private set; }
    public ParallaxBackground Background { get; private set; }
    public ParticlePool Particles { get; private set; }
    public CameraShake Shake { get; private set; }

    Camera cam;
    float inputCooldown;
    float gameOverTime;
    bool gameOverPanelShown;

    void Awake()
    {
        I = this;
        Application.targetFrameRate = 60;

        cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = GameConfig.NightSky;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();

        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(transform, false);
        Background = bgGo.AddComponent<ParallaxBackground>();
        Background.Build(cam);

        var audioGo = new GameObject("Audio");
        audioGo.transform.SetParent(transform, false);
        Audio = audioGo.AddComponent<AudioManager>();
        Audio.Init();

        var fxGo = new GameObject("Effects");
        fxGo.transform.SetParent(transform, false);
        Particles = fxGo.AddComponent<ParticlePool>();
        Particles.Init(140);

        var shakeGo = new GameObject("Shake");
        shakeGo.transform.SetParent(transform, false);
        Shake = shakeGo.AddComponent<CameraShake>();
        Shake.Init(cam);

        var playerGo = new GameObject("Player");
        playerGo.transform.SetParent(transform, false);
        Player = playerGo.AddComponent<Player>();
        Player.Init(this);

        var spawnerGo = new GameObject("Spawner");
        spawnerGo.transform.SetParent(transform, false);
        Spawner = spawnerGo.AddComponent<StackSpawner>();
        Spawner.Init(this);

        var uiGo = new GameObject("UI");
        uiGo.transform.SetParent(transform, false);
        UI = uiGo.AddComponent<UIManager>();
        UI.Build(this);

        HighScore = PlayerPrefs.GetInt(GameConfig.HighScoreKey, 0);
        SelectedCharacter = PlayerPrefs.GetInt(GameConfig.CharacterKey, 0);
        Player.SetCharacter(SelectedCharacter);

        CurrentState = State.Menu;
        UI.ShowMenu();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        var st = CurrentState;

        // background always breathes; scrolls with the world when playing
        float speed = st == State.Playing ? Spawner.CurrentSpeed : 0f;
        Background.Tick(dt, speed, st != State.GameOver);

        if (st == State.Menu)
        {
            Player.MenuBob(dt);
        }
        else if (st == State.Playing)
        {
            Player.Tick(dt);
            if (Player.Alive)
            {
                Spawner.Tick(dt);
                if (Spawner.CheckCollision(Player.Min, Player.Max))
                    Player.Die(false);
                else
                {
                    var passed = Spawner.PassedPair(GameConfig.PlayerX);
                    if (passed != null)
                    {
                        passed.Scored = true;
                        Score++;
                        UI.SetScore(Score);
                        Audio.PlayScore();
                        UI.ScorePopup(new Vector3(GameConfig.PlayerX + 0.9f, Player.transform.position.y + 0.9f, 0f));
                        Particles.Burst(Player.transform.position + Vector3.up * 0.6f,
                            new Color(0.95f, 0.85f, 0.55f), 10, 1.6f, 0.5f, -2f);
                    }
                }
            }
        }
        else // GameOver
        {
            if (!Player.DoneSinking)
                Player.TickDead(dt);
        }

        if (inputCooldown > 0f) inputCooldown -= dt;
        HandleInput(st);
    }

    void HandleInput(State st)
    {
        bool tap = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
        if (!tap && Input.touchCount > 0)
        {
            foreach (var touch in Input.touches)
                if (touch.phase == TouchPhase.Began) { tap = true; break; }
        }
        if (!tap) return;
        if (IsPointerOverUI()) return;

        Audio.StartMusic(); // browsers/platforms want a user gesture first

        if (st == State.Menu)
        {
            StartGame();
        }
        else if (st == State.Playing)
        {
            if (inputCooldown <= 0f && Player.Alive)
                Player.Flap();
        }
        else if (st == State.GameOver)
        {
            if (gameOverPanelShown && Time.time - gameOverTime > 1.0f)
                StartGame(); // quick retry
        }
    }

    bool IsPointerOverUI()
    {
        var es = UnityEngine.EventSystems.EventSystem.current;
        if (es == null) return false;
        if (Input.touchCount > 0)
        {
            foreach (var touch in Input.touches)
                if (es.IsPointerOverGameObject(touch.fingerId)) return true;
            return false;
        }
        return es.IsPointerOverGameObject();
    }

    public void StartGame()
    {
        Score = 0;
        UI.SetScore(0);
        Spawner.Begin();
        Player.ResetPlayer();
        Player.BeginPlay();
        CurrentState = State.Playing;
        gameOverPanelShown = false;
        UI.ShowHUD();
        inputCooldown = 0.25f;
    }

    public void OnPlayerDied()
    {
        if (CurrentState != State.Playing) return;
        CurrentState = State.GameOver;
        gameOverTime = Time.time;
        gameOverPanelShown = false;
        Spawner.Stop();
        Audio.PlayDie();
        Shake.AddTrauma(0.85f);

        bool newBest = Score > HighScore;
        if (newBest)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(GameConfig.HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
        StartCoroutine(ShowGameOverPanel(0.75f, newBest));
    }

    IEnumerator ShowGameOverPanel(float delay, bool newBest)
    {
        yield return new WaitForSeconds(delay);
        if (CurrentState != State.GameOver) yield break; // restarted already
        gameOverPanelShown = true;
        UI.ShowGameOver(Score, HighScore, newBest);
    }

    public void ReturnToMenu()
    {
        CurrentState = State.Menu;
        Spawner.Clear();
        Player.ResetPlayer();
        UI.ShowMenu();
    }

    public void SelectCharacter(int index)
    {
        int n = GameConfig.CharacterFileNames.Length;
        SelectedCharacter = ((index % n) + n) % n;
        PlayerPrefs.SetInt(GameConfig.CharacterKey, SelectedCharacter);
        PlayerPrefs.Save();
        Player.SetCharacter(SelectedCharacter);
        UI.RefreshCharacter(SelectedCharacter);
    }
}

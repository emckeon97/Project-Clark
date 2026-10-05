namespace RiverReel.Windows.Game;

/// <summary>
/// Flappy-style flight through the smokestacks. All units are points.
/// Direct port of the iOS FlappyEngine — same tuning.
/// The view owns the 60fps timer and calls <see cref="Update"/>; the engine
/// is UI-framework agnostic.
/// </summary>
public sealed class FlappyEngine
{
    public struct Stack
    {
        public double X;
        public double GapY;
        public double Gap;
        public bool Scored;
    }

    // MARK: - Tuning (points)

    public const double Gravity = 2300;
    public const double FlapVY = -780;
    public const double MaxFall = 1150;
    public const double PipeW = 86;
    public const double GapStart = 235;
    public const double GapMin = 185;
    public const double SpeedStart = 270;
    public const double SpeedMax = 440;
    public const double Spacing = 370;

    // MARK: - State

    public int Score { get; private set; }
    public bool Started { get; private set; }
    public bool GameOver { get; private set; }

    public double ScreenW { get; private set; } = 400;
    public double ScreenH { get; private set; } = 800;

    public double BirdX { get; private set; } = 120;
    public double BirdY { get; private set; } = 400;
    public double Vy { get; private set; }
    public double Rotation { get; private set; }  // degrees, nose-up negative
    public double FlapT { get; private set; } = 99; // seconds since last flap (wing pulse)
    public double DeadT { get; private set; }

    public List<Stack> Stacks { get; } = new();

    public const double GroundH = 96;
    public double GroundY => ScreenH - GroundH;
    public const double BirdR = 26;

    /// Seconds since run start — drives twinkle/blink in the renderer.
    public double TSec { get; private set; }

    private readonly Random _rng = new();

    private double Speed => Math.Min(SpeedMax, SpeedStart + Score * 3.5);
    private double GapFor(int s) => Math.Max(GapMin, GapStart - s * 1.2);

    // MARK: - Control

    public void Reset(double w, double h)
    {
        ScreenW = w; ScreenH = h;
        BirdX = w * 0.30;
        BirdY = h * 0.42;
        Vy = 0; Rotation = 0; FlapT = 99; TSec = 0;
        Started = false; GameOver = false; DeadT = 0;
        Score = 0;
        Stacks.Clear();
        // First pillar starts on-screen so the opening isn't a long empty flight.
        double x = w * 0.60;
        while (x < w + Spacing * 3)
        {
            Stacks.Add(NewStack(x));
            x += Spacing;
        }
    }

    private Stack NewStack(double x)
    {
        double lo = 170;
        double hi = Math.Max(lo + 1, GroundY - 170);
        return new Stack
        {
            X = x,
            GapY = lo + _rng.NextDouble() * (hi - lo),
            Gap = GapFor(Score),
        };
    }

    /// Tap!
    public void Flap()
    {
        if (GameOver) return;
        Started = true;
        Vy = FlapVY;
        FlapT = 0;
    }

    // MARK: - Simulation

    public void Update(double dt, double w, double h)
    {
        if (dt <= 0) return;
        // Adopt the real canvas size on first frames.
        if (w > 0 && (ScreenW != w || ScreenH != h))
            Reset(w, h);

        FlapT += dt;
        TSec += dt;
        if (!Started) return;

        if (GameOver)
        {
            // Tumble to the pier.
            DeadT += dt;
            Vy = Math.Min(Vy + Gravity * dt, MaxFall * 1.3);
            BirdY += Vy * dt;
            Rotation = Math.Min(90, Rotation + dt * 260);
            if (BirdY > GroundY - BirdR)
            {
                BirdY = GroundY - BirdR;
                Vy = 0;
            }
            return;
        }

        Vy = Math.Min(Vy + Gravity * dt, MaxFall);
        BirdY += Vy * dt;
        // Tilt: nose up on flap, nose down in a fall.
        double targetRot = Math.Max(-1, Math.Min(1, Vy / MaxFall)) * 38;
        Rotation += (targetRot - Rotation) * Math.Min(1, dt * 10);
        // Ceiling: bonk, don't die.
        if (BirdY < BirdR)
        {
            BirdY = BirdR;
            Vy = 0;
        }

        double dx = Speed * dt;
        for (int i = 0; i < Stacks.Count; i++)
        {
            var s = Stacks[i];
            s.X -= dx;
            Stacks[i] = s;
        }
        Stacks.RemoveAll(s => s.X < -PipeW - 40);
        double last = Stacks.Count > 0 ? Stacks.Max(s => s.X) : 0;
        if (last < ScreenW + 40)
            Stacks.Add(NewStack(last + Spacing));

        for (int i = 0; i < Stacks.Count; i++)
        {
            var s = Stacks[i];
            if (!s.Scored && s.X + PipeW < BirdX - BirdR)
            {
                s.Scored = true;
                Stacks[i] = s;
                Score++;
            }
        }

        CheckCollisions();
    }

    private void CheckCollisions()
    {
        // The pier deck.
        if (BirdY + BirdR >= GroundY)
        {
            GameOver = true;
            return;
        }
        // Smokestack slabs (circle vs. vertical slabs — corners are forgiving).
        foreach (var s in Stacks)
        {
            if (BirdX + BirdR < s.X || BirdX - BirdR > s.X + PipeW) continue;
            double top = s.GapY - s.Gap / 2;
            double bot = s.GapY + s.Gap / 2;
            if (BirdY - BirdR < top || BirdY + BirdR > bot)
            {
                GameOver = true;
                return;
            }
        }
    }
}

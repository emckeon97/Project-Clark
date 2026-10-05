using Microsoft.Maui.Storage;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using RiverReel.Windows.Game;
using RiverReel.Windows.Services;

namespace RiverReel.Windows.Views;

/// <summary>
/// The flight itself: tap/click/spacebar to flap.
/// A 60fps dispatcher timer drives the engine and invalidates the canvas.
/// </summary>
public partial class GamePage : ContentPage
{
    private readonly FlappyEngine _engine = new();
    private readonly string _characterId;
    private SKBitmap? _sprite;
    private IDispatcherTimer? _loop;
    private DateTime _lastTick;
    private bool _overFired;

    public GamePage(string characterId)
    {
        InitializeComponent();
        _characterId = characterId;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadSpriteAsync();
        MusicService.Shared.Play();
        _lastTick = DateTime.UtcNow;
        _overFired = false;
        _loop?.Stop();
        _loop = Dispatcher.CreateTimer();
        _loop.Interval = TimeSpan.FromSeconds(1.0 / 60.0);
        _loop.Tick += OnTick;
        _loop.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _loop?.Stop();
        _loop = null;
        _sprite?.Dispose();
        _sprite = null;
    }

    private async Task LoadSpriteAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync($"{_characterId}.png");
            _sprite = SKBitmap.Decode(stream);
        }
        catch
        {
            _sprite = null; // Renderer draws the fallback round bird.
        }
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        double dt = Math.Min(0.05, (now - _lastTick).TotalSeconds);
        _lastTick = now;

        double w = Canvas.CanvasSize.Width;
        double h = Canvas.CanvasSize.Height;
        _engine.Update(dt, w, h);
        Canvas.InvalidateSurface();

        if (_engine.GameOver && !_overFired)
        {
            _overFired = true;
            // Give the tumble a beat before showing THE END.
            Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(0.8), async () =>
            {
                int score = _engine.Score;
                await Navigation.PushAsync(new GameOverPage(score));
                Navigation.RemovePage(this);
            });
        }
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Black);
        FlappyRenderer.Draw(canvas, _engine, _sprite, e.Info.Width, e.Info.Height);
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        if (e.ActionType == SKTouchAction.Pressed)
        {
            _engine.Flap();
            e.Handled = true;
        }
    }
}

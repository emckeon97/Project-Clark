using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;
using RiverReel.Windows.Models;
using RiverReel.Windows.Services;
using RiverReel.Windows.Theme;

namespace RiverReel.Windows.Views;

public partial class MenuPage : ContentPage
{
    private const string SelectedKey = "clark.selected";
    private const string BestKey = "clark.best";

    private readonly List<Ellipse> _lights = new();
    private IDispatcherTimer? _lightTimer;
    private int _phase;

    public MenuPage()
    {
        InitializeComponent();
        BuildLights();
        BuildRoster();
        RefreshBest();
        RefreshMute();
        MusicService.Shared.MuteChanged += RefreshMute;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MusicService.Shared.Play();
        StartLights();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _lightTimer?.Stop();
    }

    private string SelectedId
    {
        get => Preferences.Default.Get(SelectedKey, "popeye");
        set => Preferences.Default.Set(SelectedKey, value);
    }

    // MARK: - Marquee lights

    private void BuildLights()
    {
        for (int i = 0; i < 18; i++)
        {
            var dot = new Ellipse
            {
                WidthRequest = 7, HeightRequest = 7,
                Fill = ClarkTheme.GoldM,
            };
            _lights.Add(dot);
            LightsRow.Add(dot);
        }
    }

    private void StartLights()
    {
        _lightTimer?.Stop();
        _lightTimer = Dispatcher.CreateTimer();
        _lightTimer.Interval = TimeSpan.FromSeconds(0.3);
        _lightTimer.Tick += (_, _) =>
        {
            _phase = (_phase + 1) % 3;
            for (int i = 0; i < _lights.Count; i++)
                _lights[i].Opacity = (i + _phase) % 3 == 0 ? 0.22 : 1.0;
        };
        _lightTimer.Start();
    }

    // MARK: - Roster

    private void BuildRoster()
    {
        RosterRow.Clear();
        foreach (var toon in Roster.All)
        {
            bool isSel = toon.Id == SelectedId;
            var img = new Image
            {
                WidthRequest = 60, HeightRequest = 60,
                Aspect = Aspect.AspectFit,
            };
            // Sprites are MauiAssets in Resources/Raw (copied over from the iOS project).
            _ = LoadSpriteAsync(toon.Id, img);

            var frame = new Frame
            {
                WidthRequest = 72, HeightRequest = 72,
                CornerRadius = 36,
                BorderColor = isSel ? ClarkTheme.GoldM : ClarkTheme.CreamM.WithAlpha(0.25f),
                BackgroundColor = ClarkTheme.CreamM.WithAlpha(0.06f),
                Padding = 6,
                HasShadow = false,
                Content = img,
            };
            var tap = new TapGestureRecognizer();
            string id = toon.Id;
            tap.Tapped += (_, _) => { SelectedId = id; BuildRoster(); };
            frame.GestureRecognizers.Add(tap);
            RosterRow.Add(frame);
        }
        ToonNameLabel.Text = (Roster.ById(SelectedId)?.Name ?? "").ToUpperInvariant();
    }

    private static async Task LoadSpriteAsync(string id, Image img)
    {
        try
        {
            var stream = await FileSystem.OpenAppPackageFileAsync($"{id}.png");
            img.Source = ImageSource.FromStream(() => stream);
        }
        catch
        {
            // Sprite not bundled yet — circle stays empty.
        }
    }

    private void RefreshBest()
        => BestLabel.Text = $"★ BEST {Preferences.Default.Get(BestKey, 0)}";

    private void RefreshMute()
        => MuteButton.Text = MusicService.Shared.IsMuted ? "🔇" : "🔊";

    private void OnMuteClicked(object? sender, EventArgs e)
        => MusicService.Shared.ToggleMute();

    private async void OnPlayClicked(object? sender, EventArgs e)
        => await Navigation.PushAsync(new GamePage(SelectedId));
}

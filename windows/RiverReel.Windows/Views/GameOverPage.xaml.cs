using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;
using RiverReel.Windows.Theme;

namespace RiverReel.Windows.Views;

/// <summary>Silent-film "THE END" card with score, best, and retry.</summary>
public partial class GameOverPage : ContentPage
{
    private const string BestKey = "clark.best";
    private const string SelectedKey = "clark.selected";

    private readonly int _score;
    private readonly List<Ellipse> _lights = new();
    private IDispatcherTimer? _lightTimer;
    private int _phase;

    public GameOverPage(int score)
    {
        InitializeComponent();
        _score = score;

        int best = Preferences.Default.Get(BestKey, 0);
        bool isRecord = score > best;
        if (isRecord && score > 0)
            Preferences.Default.Set(BestKey, score);

        ScoreLabel.Text = score.ToString();
        RecordLabel.IsVisible = isRecord && score > 0;
        BestLineLabel.Text = $"BEST {(isRecord ? score : best)}";

        for (int i = 0; i < 14; i++)
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _lightTimer?.Stop();
    }

    private async void OnRetryClicked(object? sender, EventArgs e)
    {
        string id = Preferences.Default.Get(SelectedKey, "popeye");
        await Navigation.PushAsync(new GamePage(id));
        Navigation.RemovePage(this);
    }

    private async void OnMenuClicked(object? sender, EventArgs e)
        => await Navigation.PopToRootAsync();
}

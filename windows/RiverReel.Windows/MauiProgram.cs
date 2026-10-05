using SkiaSharp.Views.Maui.Controls.Hosting;

namespace RiverReel.Windows;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                // System serif is used for the marquee look; add custom fonts here if desired.
            });

        return builder.Build();
    }
}

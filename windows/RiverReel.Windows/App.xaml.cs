using RiverReel.Windows.Views;

namespace RiverReel.Windows;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new MenuPage());
    }
}

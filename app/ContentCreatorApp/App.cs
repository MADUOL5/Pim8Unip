namespace ContentCreatorApp;

public sealed class App : Application
{
    private readonly MainPage _mainPage;

    public App(MainPage mainPage)
    {
        _mainPage = mainPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var navigationPage = new NavigationPage(_mainPage)
        {
            BarBackgroundColor = Color.FromArgb("#151515"),
            BarTextColor = Colors.White
        };

        return new Window(navigationPage)
        {
            Title = "Paqueta's Streaming Creator"
        };
    }
}

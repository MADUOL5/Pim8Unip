using ContentCreatorApp.Services;

namespace ContentCreatorApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<StreamingApiClient>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}

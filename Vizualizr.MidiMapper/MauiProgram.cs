using Microsoft.Extensions.Logging;
using Vizualizr.Backend.Midi;
using Vizualizr.MidiMapper.ViewModels;
using CommunityToolkit.Maui;

namespace Vizualizr.MidiMapper;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddSingleton<DeviceService>();
        builder.Services.AddSingleton<MidiIO>();
        builder.Services.AddSingleton<MainPageMidiHandler>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
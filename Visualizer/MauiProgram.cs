using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Services;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ViewModels;
using ViewModels.FileBrowser;

namespace Visualizer;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.UseSkiaSharp();
        builder.UseMauiCommunityToolkit();

        AddServices(builder.Services);
        AddViewModels(builder.Services);
        AddViews(builder.Services);
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void AddServices(IServiceCollection builderServices)
    {
        builderServices.AddSingleton<TrackManager>();
        builderServices.AddSingleton<StatusService>();
        builderServices.AddSingleton<IMessenger, WeakReferenceMessenger>();
    }
    
    private static void AddViewModels(IServiceCollection builderServices)
    {
        builderServices.AddSingleton<FileBrowserViewModel>();
        builderServices.AddSingleton<MainPageViewModel>();

        builderServices.AddTransient<TrackViewModel>();
    }

    private static void AddViews(IServiceCollection builderServices)
    {
        builderServices.AddSingleton<MainPage>();
    }
}
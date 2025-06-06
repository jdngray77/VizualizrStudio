﻿using System.Runtime.InteropServices;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using ServiceInterfaces;
using Vizualizr.Backend;
using Vizualizr.Backend.Audio;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Core.Hosting;
using ViewModels;
using ViewModels.FileBrowser;
using Vizualizr.Backend.Audio.Player;

namespace Vizualizr;

public static class MauiProgram
{

#if WINDOWS
    [DllImport("user32.dll", SetLastError = true)]
    static extern int ShowWindow(IntPtr hWnd, int cmd);
#endif
    
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

#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>  
        {  
            events.AddWindows(wndLifeCycleBuilder =>  
            {  
                wndLifeCycleBuilder.OnWindowCreated(window =>  
                {  
                    IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);  
                    ShowWindow(hWnd, 3);  
                    window.ExtendsContentIntoTitleBar = true;  
                    WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);  
                    var _appWindow = AppWindow.GetFromWindowId(myWndId);  
                    _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                });  
            });  
        });  
#endif


        builder.UseSkiaSharp();
        builder.UseMauiCommunityToolkit();
        builder.ConfigureSyncfusionCore();

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
        builderServices.AddSingleton<TrackHypervisor>();
        builderServices.AddSingleton<StatusService>();
        builderServices.AddSingleton<AudioHypervisor>();
        builderServices.AddSingleton<StartupService>();
        builderServices.AddSingleton<DeckManager>();
        builderServices.AddSingleton<INonCommonServices, MauiServices.MauiServices>();
        builderServices.AddSingleton<IMessenger, WeakReferenceMessenger>();
    }
    
    private static void AddViewModels(IServiceCollection builderServices)
    {
        builderServices.AddSingleton<FileBrowserViewModel>();
        builderServices.AddSingleton<MainPageViewModel>();
        builderServices.AddSingleton<TitleBarViewModel>();
        builderServices.AddTransient<TrackViewModel>();
    }

    private static void AddViews(IServiceCollection builderServices)
    {
        builderServices.AddSingleton<MainPage>();
    }
}
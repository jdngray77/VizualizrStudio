using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vizualizr.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }
    
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    static readonly IntPtr HWND_TOPMOST = new IntPtr(0);
    const uint SWP_NOMOVE = 0x0002;
    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOACTIVATE = 0x0010;
    const uint SWP_SHOWWINDOW = 0x0040;
    
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);
    
    [DllImport("user32.dll")]
    static extern bool AllowSetForegroundWindow(uint dwProcessId);


    void BringToFront(MauiWinUIWindow window)
    {
        var hwnd = WindowNative.GetWindowHandle(window);
        var s = SetForegroundWindow(hwnd);
    }

    public void MakeWindowTopMost(MauiWinUIWindow window)
    {
        var hwnd = WindowNative.GetWindowHandle(window);
        bool x = SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Let this process set foreground
        var s = AllowSetForegroundWindow(Convert.ToUInt32(Process.GetCurrentProcess().Id));
        
        var splash = new SplashWindow();
        splash.Activate();

        // Delay asynchronously, keeping the main thread alive
        await Task.Delay(1000).ConfigureAwait(true);

        bool visible = true;
        
        Task.Delay(100).ContinueWith(async _ =>
        {
            while (visible)
            {
                await Task.Delay(100);
                MakeWindowTopMost(splash);
                BringToFront(splash);
            }
        });

        
        Task.Delay(3000).ContinueWith(_ =>
        {
            return splash.DispatcherQueue.TryEnqueue(() =>
            {
                splash.Close(); 
                visible = false;
            });
        });
        
        base.OnLaunched(args); // Start the MAUI app AFTER splash closes
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
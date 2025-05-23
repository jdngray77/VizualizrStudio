using System.Runtime.InteropServices;
using Windows.Graphics;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Grid = Microsoft.UI.Xaml.Controls.Grid;
using Stretch = Microsoft.UI.Xaml.Media.Stretch;

namespace Visualizer.WinUI;

public class SplashWindow : MauiWinUIWindow
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    const int GWL_STYLE = -16;
    const int WS_SYSMENU = 0x00080000;
    const int WS_CAPTION = 0x00C00000;
    const int WS_THICKFRAME = 0x00040000;
    const int WS_MINIMIZEBOX = 0x00020000;
    const int WS_MAXIMIZEBOX = 0x00010000;

    public SplashWindow()
    {
        const int splashWidth = 400;
        const int splashHeight = 300;

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        // Remove all window styles
        int style = GetWindowLong(hwnd, GWL_STYLE);
        style &= ~(WS_SYSMENU | WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        SetWindowLong(hwnd, GWL_STYLE, style);

        // Set up the image content
        var img = new BitmapImage(new Uri("ms-appx:///Resources/Images/splash.png"));
        
        appWindow.Resize(new SizeInt32(1, 1));
        appWindow.Move(new PointInt32(1, 1));
        
        img.ImageOpened += (s, e) =>
        {
            int width = img.PixelWidth / 4;
            int height = img.PixelHeight / 4;

            // Resize and center window based on image size
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            var workArea = displayArea.WorkArea;
            int x = workArea.X + (workArea.Width - width) / 2;
            int y = workArea.Y + (workArea.Height - height) / 2;

            appWindow.Resize(new SizeInt32(width, height));
            appWindow.Move(new PointInt32(x, y));
        };
        
        Microsoft.UI.Xaml.Controls.Image image = new Microsoft.UI.Xaml.Controls.Image()
        {
            Source = img,
            Stretch = Stretch.Fill
        };

        var grid = new Grid();
        grid.Children.Add(image);

        this.Content = grid;
    }
}
using System.Runtime.InteropServices;

#if WINDOWS
using WinRT.Interop;
#endif

namespace Vizualizr
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
        
        
    }
}
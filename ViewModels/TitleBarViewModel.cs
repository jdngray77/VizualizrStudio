using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using ServiceInterfaces;
using System.Diagnostics;

namespace ViewModels
{
    [ObservableObject]
    public partial class TitleBarViewModel
    {
        private IAlertService alertService;

        public TitleBarViewModel(IAlertService alertService)
        {
            this.alertService = alertService;
        }


        [ObservableProperty]
        private bool isDebuggerConnected = Debugger.IsAttached;

        [ObservableProperty]
        private string currentTime;

        [ObservableProperty]
        private string batteryPercent;

        [RelayCommand]
        private void DebuggerBreak()
        {
            Debugger.Break();
        }

        [RelayCommand]
        private async Task Quit()
        {
            var quit = await alertService.DisplayAlert("Do you really want to quit?", string.Empty, "Yes", "No")
                .ConfigureAwait(false);

            if (quit)
            {
               Environment.Exit(0);
            }
        }

        [RelayCommand]
        private void Minimise()
        {

        }

        [RelayCommand]
        private void ToggleFullscreen()
        {

        }
    }
}

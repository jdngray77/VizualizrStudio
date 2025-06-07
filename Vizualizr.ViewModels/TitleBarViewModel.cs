using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ServiceInterfaces;
using System.Diagnostics;
using Vizualizr.Backend.Messaging;

namespace ViewModels
{
    [ObservableObject]
    public partial class TitleBarViewModel
    {
        private INonCommonServices services;
        private IMessenger messenger;

        public TitleBarViewModel(
            INonCommonServices services,
            IMessenger messenger)
        {
            this.services = services;
            this.messenger = messenger;
            messenger.Register<BackgroundUpdate>(this, this.BackgroundUpdate);
            BackgroundUpdate(null, null);
        }

        [ObservableProperty]
        private bool isDebuggerConnected = Debugger.IsAttached;

        [ObservableProperty]
        private DateTime currentTime;

        [ObservableProperty]
        private string batteryPercent;

        [ObservableProperty]
        private bool onBatteryPower;

        [RelayCommand]
        private void DebuggerBreak()
        {
            Debugger.Break();
        }

        [RelayCommand]
        private async Task Quit()
        {
            var quit = await services.DisplayAlert("Do you really want to quit?", string.Empty, "Yes", "No")
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

        private void BackgroundUpdate(object recipient, BackgroundUpdate message)
        {
            CurrentTime = DateTime.Now;
            BatteryPercent = services.GetBatteryPercentage() + "%";
            OnBatteryPower = services.OnBatteryPower();
        }
    }
}

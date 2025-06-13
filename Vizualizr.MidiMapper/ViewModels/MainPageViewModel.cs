using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Vizualizr.Backend.Midi;

namespace Vizualizr.MidiMapper.ViewModels
{
    [ObservableObject]
    public partial class MainPageViewModel
    {
        private MidiIO io;

        public MainPageViewModel(MidiIO io)
        {
            this.io = io;

            for (byte i = 0; i < 20; i++)
            {
                InputMappings.Add(new InputMappingViewModel()
                {
                    Action = "test",
                    Channel = i,
                    Velocity = i,
                    Control = i,
                    Type = EInputMappingType.Control,
                });

                OutputMappings.Add(new InputMappingViewModel()
                {
                    Action = $"LED {i}",
                    Channel = i,
                    Velocity = i,
                    Control = i,
                    Type = EInputMappingType.Control,
                });
            }

            InputMappings.Add(new InputMappingViewModel()
            {
                Action = "real test",
                Channel = 1,
                Velocity = 0,
                Control = 50,
                Type = EInputMappingType.Note,
            });

            Task.Run(async () =>
            {
                await Task.Delay(1000).ConfigureAwait(false);
                Load();
            });
        }

        public ObservableCollection<InputMappingViewModel> InputMappings { get; init; } = new ObservableCollection<InputMappingViewModel>();
        public ObservableCollection<InputMappingViewModel> OutputMappings { get; init; } = new ObservableCollection<InputMappingViewModel>();
        public ObservableCollection<string> DeviceNames { get; init; } = new ObservableCollection<string>();

        [ObservableProperty]
        private InputMappingViewModel? selectedInputMapping = null;

        [ObservableProperty]
        private string selectedDeviceName;

        [ObservableProperty]
        private bool deviceNotConnected = true;

        [ObservableProperty]
        private string lastInput;

        [RelayCommand]
        public async Task RefreshDevices()
        {
            var devices = io.GetDeviceNames();

            if (devices.All(device => DeviceNames.Contains(device)))
            {
                // No changes!
                return;
            };

            DeviceNames.Clear();

            // No Devices!
            if (devices.Count() == 0)
            {
                return;
            }

            foreach (var item in devices)
            {
                DeviceNames.Add(item);
            }

            // Without wiping selected device (as device may not be present any more)
            // Choose the first device if we don't already have one.
            if (String.IsNullOrEmpty(selectedDeviceName))
            {
                SelectedDeviceName = devices.First();
            }
        }

        [RelayCommand]
        public async Task OpenMappingWizard()
        {
            Debug.WriteLine("Open mapping wizard");
        }


        [RelayCommand]
        public async Task TestMapping()
        {
            Debug.WriteLine("test mapping");

        }

        [RelayCommand]
        public async Task NewMapping()
        {
            Debug.WriteLine("new mapping");

        }

        [RelayCommand]
        public async Task SaveMapping()
        {
            Debug.WriteLine("Save Mapping");

        }

        [RelayCommand]
        public async Task SaveMappingAs()
        {
            Debug.WriteLine("Save As");
        }

        [RelayCommand]
        public async Task AddInputMapping()
        {
            Debug.WriteLine("Add Input");
        }

        [RelayCommand]
        public async Task AddOutputMapping()
        {
            Debug.WriteLine("Add Output");
        }

        partial void OnSelectedDeviceNameChanged(string value)
        {
            DeviceNotConnected = !io.GetDeviceNames().Contains(value);
        }

        private async Task Load()
        {
            await RefreshDevices().ConfigureAwait(false);
        }
    }
}

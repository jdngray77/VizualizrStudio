using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;
using CommunityToolkit.Maui.Storage;
using Vizualizr.Backend.Midi;
using Vizualizr.Backend.Midi.FileModel;
using Vizualizr.Backend.Midi.Model;
using Vizualizr.Backend.Utility;
using CommunityToolkit.Maui.Alerts;
using Vizualizr.MidiMapper.Utility;

namespace Vizualizr.MidiMapper.ViewModels
{
    [ObservableObject]
    public partial class MainPageViewModel
    {
        private MidiIO io;
        private MauiServices services;

        /// <summary>
        /// When <see cref="Message"/> is set, this timer
        /// is started and used to clear that message.
        /// </summary>
        private ResettableTimer clearMessageTimer = new(5000);

        /// <summary>
        /// If loaded a mapping from a file, or has been saved,
        /// this contains the path to that file such that we can just update
        /// the existing file.
        /// </summary>
        private string? filePath = null;

        public MainPageViewModel(MauiServices services, MidiIO io)
        {
            this.services = services;
            this.io = io;

            Task.Run(async () =>
            {
                await Task.Delay(1000).ConfigureAwait(false);
                await Load().ConfigureAwait(false);
            });

            clearMessageTimer.TimerElapsedWithoutReset += () =>
            {
                Message = string.Empty;
            };
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

        [ObservableProperty]
        private string message;

        partial void OnMessageChanged(string value)
        {
            clearMessageTimer.Reset();
        }

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
        public async Task Open()
        {
            try
            {
                var fileResult = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a Midi Mapping File",
                    FileTypes = FileType.MidiMap
                });

                if (fileResult == null)
                {
                    Debug.WriteLine("User cancelled file picking.");
                    return;
                }


                filePath = fileResult.FullPath;
                using var stream = await fileResult.OpenReadAsync();

                var serializer = new XmlSerializer(typeof(MidiMapping));
                if (serializer.Deserialize(stream) is not MidiMapping mapping)
                {
                    await Toast.Make("Failed to read mapping file.").Show();
                    return;
                }


                this.InputMappings.Clear();

                foreach (var item in mapping.Inputs)
                {
                    InputMappings.Add(new InputMappingViewModel()
                    {
                        Deck = item.Deck,
                        Control = item.Control,
                        Channel = item.Channel,
                        Command = item.MapsTo,
                        IsAbsolute = item is ControlInputMapping c ? c.Absolute : false,
                        MinimumValue = item is ControlInputMapping a ? a.MinimumValue : (byte)0,
                        MaximumValue = item is ControlInputMapping b ? b.MaximumValue : (byte)127,
                        Type = item is ControlInputMapping ? EInputMappingType.CC : EInputMappingType.Note,
                        UsesVelocityInsteadOfNoteOff = item is NoteInputMapping noteInput ? noteInput.UsesVelocityInsteadOfNoteOff : false
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening mapping file: {ex.Message}");
                await Toast.Make("Error opening file.").Show();
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
            if (String.IsNullOrEmpty(filePath))
            {
                return;
            }

            try
            {
                Debug.WriteLine("Save");

                var mapping = CreateMappingFile();

                var serializer = new XmlSerializer(typeof(MidiMapping));

                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                using var writer = new StreamWriter(fileStream, Encoding.UTF8);
                serializer.Serialize(writer, mapping);

              Debug.WriteLine($"Saved to: filePath");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving XML: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task SaveMappingAs()
        {
            try
            {
                Debug.WriteLine("Save As");

                var mapping = CreateMappingFile();

                var serializer = new XmlSerializer(typeof(MidiMapping));

                using var memoryStream = new MemoryStream();
                using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
                {
                    serializer.Serialize(writer, mapping);
                    writer.Flush();
                }

                memoryStream.Position = 0;

                var result = await FileSaver.Default.SaveAsync(
                    $"{selectedDeviceName.Trim()}.mmap",
                    memoryStream).ConfigureAwait(false);

                if (result.IsSuccessful)
                {
                    Debug.WriteLine($"Saved to: {result.FilePath}");
                }
                else
                {
                    Debug.WriteLine("User cancelled save.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving XML: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task AddInputMapping()
        {
            if (string.IsNullOrEmpty(SelectedDeviceName))
            {
                Message = "You need to select a device before you can add a mapping!";
                return;
            }

            var newMapping = await services.DisplayAddInputMappingDialog(SelectedDeviceName).ConfigureAwait(false);

            if (newMapping == null)
            {
                return;
            }
            
            InputMappings.Add(newMapping);
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
        
        private MidiMapping CreateMappingFile()
        {
            var midiMapping = new MidiMapping()
            {
                Meta = new Meta()
                {
                    DeviceName = SelectedDeviceName
                },
            };

            foreach (var vm in InputMappings)
            {
                if (vm.Type == EInputMappingType.CC)
                {
                    midiMapping.Inputs.Add(new ControlInputMapping()
                    {
                        Channel = vm.Channel,
                        Control = vm.Control,
                        Absolute = vm.IsAbsolute,
                        MinimumValue = vm.MinimumValue,
                        MaximumValue = vm.MaximumValue,
                        MapsTo = vm.Command,
                        Deck = vm.Deck
                    });                    
                }
                else
                {
                    midiMapping.Inputs.Add(new NoteInputMapping()
                    {
                        Channel = vm.Channel,
                        Control = vm.Control,
                        UsesVelocityInsteadOfNoteOff = vm.UsesVelocityInsteadOfNoteOff
                    });  
                }
            }

            return midiMapping;
        }
    }
}

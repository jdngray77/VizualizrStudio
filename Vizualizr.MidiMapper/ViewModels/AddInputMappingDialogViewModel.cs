using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Vizualizr.Backend.Midi;
using Vizualizr.Backend.Midi.CommandProcessing;
using Vizualizr.MidiMapper.MIDI;

namespace Vizualizr.MidiMapper.ViewModels
{
    /// <summary>
    /// View model for a modal dialog which creates a new input
    /// mapping.
    /// </summary>
    [ObservableObject]
    public partial class AddInputMappingDialogViewModel
    {
        MidiIO io;
        AddInputMappingDialogMidiHandler midiHandler;

        /// <summary>
        /// View model for a modal dialog which creates a new input
        /// mapping. 
        /// </summary>
        /// <param name="io">For listening to midi inputs to determine the control to map.</param>
        public AddInputMappingDialogViewModel(MidiIO io)
        {
            Init();

            this.io = io;
            midiHandler = new AddInputMappingDialogMidiHandler(this);
            
            // Is de-registered when the dialog is closed.
            io.RegisterHandler(midiHandler);
        }

        /// <summary>
        /// The name of the device that this dialog will exclusively listen for
        /// inputs from.
        ///
        /// Set by the maui services when the dialog is shown.
        /// </summary>
        [ObservableProperty]
        private string deviceName;

        /// <summary>
        /// A list of distinct control inputs received.
        ///
        /// When a midi input is recieved, it's added here for a user to select one.
        ///
        /// A selected input can be placed in <see cref="SelectedInput"/> to automatically
        /// update the <see cref="Channel"/> and <see cref="control"/>
        /// </summary>
        public ObservableCollection<InputMappingViewModel> Inputs { get; } = new();

        /// <summary>
        /// Clears <see cref="Inputs"/>.
        /// </summary>
        [RelayCommand]
        private async Task ClearInputs()
        {
            Inputs.Clear();
        }
        
        /// <summary>
        /// For aiding the user in determining the midi information,
        /// place an input from <see cref="Inputs"/> in here to automatically populate
        /// the <see cref="Channel"/>, <see cref="Control"/> and <see cref="Type"/>
        /// </summary>
        [ObservableProperty]
        private InputMappingViewModel? selectedInput = null;

        /// <summary>
        /// Updates <see cref="Channel"/>, <see cref="Control"/> and <see cref="Type"/>
        /// to the selected input received from the device.
        /// </summary>
        /// <param name="value"></param>
        partial void OnSelectedInputChanged(InputMappingViewModel? value)
        {
            if (value == null)
            {
                return;
            }

            SelectedInputType = value.Type;
            Control = value.Control;
            Channel = value.Channel;
        }

        /// <summary>
        /// The list of Vizualizr commands that the MIDI input can map to.
        ///
        /// Place the selected command in <see cref="Command"/>
        /// </summary>
        public ObservableCollection<EDeckCommand> AvailableCommands { get; } = new();
        
        /// <summary>
        /// Required.
        ///
        /// Determines the Vizualizr command that this midi input will
        /// correspond to.
        /// </summary>
        [ObservableProperty] 
        [NotifyPropertyChangedFor(nameof(IsButton))]        // Because determines type of control
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]    // Because is a required property.
        private EDeckCommand selectedCommand;
        
        /// <summary>
        /// True if the <see cref="SelectedCommand"/> contains the
        /// <see cref="EDeckCommand.Private_ButtonFlag"/> flag, which indicates
        /// that the control command is a button / trigger - rather than a numeric input.
        /// </summary>
        [ObservableProperty]
        private bool isButton;
        
        partial void OnSelectedCommandChanged(EDeckCommand value)
        {
            IsButton = value.IsButton();
        }
        
        /// <summary>
        /// Determines the deck that will be subject to this command.
        ///
        /// May be ignored if the command in use does not target a deck,
        /// such as a command for the track chooser.
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]    // Because at present is required, as all commands target a deck.
        private byte deck = 0;
        
        /// <summary>
        /// The possible types of inputs that can be mapped.
        ///
        /// We support buttons (Notes) and controls (CC)
        /// </summary>
        public ObservableCollection<EInputMappingType> AvailableInputTypes { get; } = new();

        /// <summary>
        /// The type of MIDI input from the controller.
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        private EInputMappingType selectedInputType;

        /// <summary>
        /// The channel in which the input is found on the controller.
        /// </summary>
        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        private byte channel;
        
        /// <summary>
        /// Basic bounds checking to keep channel within the midi range.
        /// </summary>
        /// <param name="value"></param>
        partial void OnChannelChanged(byte value)
        {
            if (value < 1)
            {
                Channel = 1;
            }

            if (value > 16)
            {
                Channel = 16;
            }
        }

        /// <summary>
        /// The identifier for the control or note on the controller.
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        private byte control;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        partial void OnControlChanged(byte value)
        {
            if (value == null)
            {
                return;
            }

            if (value < 1)
            {
                Control = 1;
            }

            if (value > 127)
            {
                Control = 127;
            }
        }

        /// <summary>
        /// If true, indicates that when handled the control's
        /// velocity should be inversed.
        ///
        /// Only for CC controls. Notes do not utilise this.
        /// </summary>
        [ObservableProperty] 
        private bool invertVelocity;
        
        /// <summary>
        /// If true, indicates that the CC has end limits, and does not physically
        /// continuously rotate.
        ///
        /// If false, the velocity shall be read as a relative value to the control's current position.
        /// </summary>
        [ObservableProperty]
        private bool isAbsolute;

        /// <summary>
        /// the smallest value that can be sent to Vizualizr
        /// when this input is received.
        /// </summary>
        [ObservableProperty]
        private byte minimumValue = 0;
        
        /// <summary>
        /// the largest value that can be sent to Vizualizr
        /// when this input is received.
        /// </summary>
        [ObservableProperty]
        private byte maximumValue = 127;
        
        /// <summary>
        /// Notifies the view that we are done, and the modal can be closed.
        ///
        /// the parameter is the result of the dialog, which may be null
        /// if the user cancels.
        /// </summary>
        public event EventHandler<InputMappingViewModel?> CloseModal;

        /// <summary>
        /// Closes the dialog with no result.
        /// </summary>
        [RelayCommand]
        private async Task Cancel()
        {
            CloseModal?.Invoke(this, null);
            io.DeregisterHandler(midiHandler);
        }

        /// <summary>
        /// Submits the dialog, with the response.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanAdd))]
        private void Add()
        {
            // Can add wasn't working correctly in maui, so i've
            // just put the checks here. whatevs.
            
            if (SelectedCommand == null)
            {
                return;
            }

            if (deck == null || deck < 0 || deck > 6)
            {
                return;
            }

            if (Channel == null || channel > 16 || channel < 1)
            {
                return;
            }

            if (Control == null || Control > 127 || Control < 1)
            {
                return;
            }

            var result = new InputMappingViewModel()
            {
                Deck = Deck,
                Command = SelectedCommand,
                Channel = Channel,
                Control = Control,
                Type = SelectedInputType
            };

            CloseModal?.Invoke(this, result);
            io.DeregisterHandler(midiHandler);
        }

        private bool CanAdd()
        {
            //if (command == null)
            //{
            //    return false;
            //}

            //if (deck == null || deck == 0 || deck > 6 || deck < 1)
            //{
            //    return false;
            //}

            //if (Channel == null || channel > 127 || channel < 1)
            //{
            //    return false;
            //}

            //if (Control == null || Control > 127 || Control < 1)
            //{
            //    return false;
            //}

            return true;
        }

        /// <summary>
        /// Used by <see cref="midiHandler"/> to notify the view model
        /// of new midi inputs received whilst the dialog is open.
        /// </summary>
        /// <param name="newInput"></param>
        internal void ReceivedNewInput(InputMappingViewModel newInput)
        {
            var existing = Inputs.FirstOrDefault(it => it.Channel == newInput.Channel && it.Control == newInput.Control);
            
                
            if (existing == null)
            {
                Inputs.Add(newInput);
                existing = newInput;
            }

            SelectedInput = existing;
        }

        private void Init()
        {
            PopulateCommands();
            PopulateInputTypes();
        }

        private void PopulateInputTypes()
        {
            var inputTypes = Enum.GetValues<EInputMappingType>();

            AvailableInputTypes.Clear();

            foreach (var type in inputTypes)
            {
                AvailableInputTypes.Add(type);
            }

            SelectedInputType = AvailableInputTypes.FirstOrDefault();
        }

        private void PopulateCommands()
        {
            var eDeckCommands = Enum.GetValues<EDeckCommand>();

            var nonPrivateCommands = eDeckCommands.Where(it => !Enum.GetName(it).StartsWith("Private"));
            
            AvailableCommands.Clear();
            
            foreach (var nonPrivateCommand in nonPrivateCommands)
            {
                AvailableCommands.Add(nonPrivateCommand);
            }

            SelectedCommand = AvailableCommands.FirstOrDefault();
        }
    }
}
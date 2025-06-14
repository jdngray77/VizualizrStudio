using CommunityToolkit.Mvvm.ComponentModel;
using Vizualizr.Backend.Midi.CommandProcessing;

namespace Vizualizr.MidiMapper.ViewModels
{
    [ObservableObject]
    public partial class InputMappingViewModel
    {
        [ObservableProperty]
        private byte deck;
        
        [ObservableProperty]
        private EDeckCommand command;

        [ObservableProperty]
        private byte channel;

        [ObservableProperty]
        private byte control;

        [ObservableProperty]
        private byte velocity;

        [ObservableProperty]
        private EInputMappingType type;

        [ObservableProperty] 
        private bool isAbsolute = false;
        
        [ObservableProperty]
        private byte minimumValue;
        
        [ObservableProperty]
        private byte maximumValue;
        
        [ObservableProperty]
        private bool usesVelocityInsteadOfNoteOff;
    }
}

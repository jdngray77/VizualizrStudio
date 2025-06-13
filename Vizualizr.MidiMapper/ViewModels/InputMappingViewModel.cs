using CommunityToolkit.Mvvm.ComponentModel;

namespace Vizualizr.MidiMapper.ViewModels
{

    [ObservableObject]
    public partial class InputMappingViewModel
    {

        [ObservableProperty]
        private string action;

        [ObservableProperty]
        private byte channel;

        [ObservableProperty]
        private byte control;

        [ObservableProperty]
        private byte velocity;

        [ObservableProperty]
        private EInputMappingType type;
    }
}

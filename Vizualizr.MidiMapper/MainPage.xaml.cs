using Vizualizr.MidiMapper.ViewModels;

namespace Vizualizr.MidiMapper;

public partial class MainPage : ContentPage
{
    public MainPage(
        MainPageViewModel viewModel,
        MainPageMidiHandler mainPageHandler,
        DeviceService deviceService)
    {
        InitializeComponent();

        BindingContext = viewModel;

        deviceService.AddHandler(mainPageHandler);
        deviceService.Initialize();
    }
}
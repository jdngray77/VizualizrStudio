using Vizualizr.MidiMapper.ViewModels;

namespace Vizualizr.MidiMapper;

public partial class MainPage : ContentPage
{
    MainPageViewModel vm;

    public MainPage(
        MainPageViewModel viewModel,
        MainPageMidiHandler mainPageHandler,
        DeviceService deviceService)
    {
        this.vm = viewModel;

        InitializeComponent();

        BindingContext = viewModel;

        deviceService.AddHandler(mainPageHandler);
        deviceService.Initialize();

        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        InputMappingList.SelectionChanged += InputMappingList_SelectionChanged;
    }

    private bool updatingSelectionFromClick = false;
    private void InputMappingList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        updatingSelectionFromClick = true;
        vm.SelectedInputMapping = (InputMappingViewModel)e.CurrentSelection.FirstOrDefault();
        updatingSelectionFromClick = false;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Bodge for a maui bug where styles are not correctly applied to a
        // selected item when it is set from the view model, but is when
        // selected by the user.
        if (e.PropertyName == nameof(MainPageViewModel.SelectedInputMapping))
        {
            if (updatingSelectionFromClick)
            {
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Force UI update by resetting selection temporarily
                var selected = InputMappingList.SelectedItem;
                InputMappingList.SelectedItem = null;

                // Needed: delay re-setting to ensure layout/virtualization catches up
                Task.Delay(100).ContinueWith(_ =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        InputMappingList.SelectedItem = vm.SelectedInputMapping;
                        InputMappingList.ScrollTo(vm.SelectedInputMapping, position: ScrollToPosition.MakeVisible);
                    });
                });
            });
        }
    }
}
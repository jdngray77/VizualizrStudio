using Vizualizr.MidiMapper.ViewModels;

namespace Vizualizr.MidiMapper.Views;

public partial class AddInputMappingDialog : ContentPage
{
    public AddInputMappingDialog(AddInputMappingDialogViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.CloseModal += CloseModal;
    }

    private readonly TaskCompletionSource<InputMappingViewModel?> _tcs = new();

    public Task<InputMappingViewModel?> ShowAsync()
    {
        return _tcs.Task;
    }

    private async void CloseModal(object sender, InputMappingViewModel? e)
    {
        _tcs.SetResult(e);
        await Navigation.PopModalAsync(animated: true);
    }
}
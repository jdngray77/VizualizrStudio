namespace Vizualizr.MidiMapper.Views;

public partial class ModalAlert : ContentPage
{

    public ModalAlert(string title, string message, string yesText = "OK", string noText = "Cancel")
    {
        InitializeComponent();
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        YesButton.Text = yesText;
        NoButton.Text = noText;
    }

    private readonly TaskCompletionSource<bool> _tcs = new();

    public Task<bool> ShowAsync()
    {
        return _tcs.Task;
    }

    private async void OnYesClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(true);
        await CloseModalAsync();
    }

    private async void OnNoClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(false);
        await CloseModalAsync();
    }

    private async Task CloseModalAsync()
    {
        await Navigation.PopModalAsync(animated: true); 
    }
}
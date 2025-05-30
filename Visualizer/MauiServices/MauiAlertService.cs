using ServiceInterfaces;
using Visualizer.Views;

namespace Visualizer.MauiServices
{
    internal class MauiAlertService : IAlertService
    {
        public async Task<bool> DisplayAlert(string title, string body, string yes, string no)
        {
            var alert = new ModalAlert(title, body, yes, no);
            await Shell.Current.Navigation.PushModalAsync(alert);
            bool result = await alert.ShowAsync();
            return result;
        }
    }
}

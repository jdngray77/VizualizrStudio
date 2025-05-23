using CommunityToolkit.Mvvm.Messaging;
using Services.Messages;
using ViewModels;

namespace Visualizer
{
    public partial class MainPage
    {
        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Application.Current.Handler.MauiContext.Services.GetRequiredService<IMessenger>().Send<ShouldInitializeStatus>();
        }
    }
}
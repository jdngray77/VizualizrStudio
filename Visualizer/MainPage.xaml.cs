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

            // TODO no.
            Mix.DeckA.BindingContext = vm.PlayerA;
            Mix.DeckB.BindingContext = vm.PlayerB;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Application.Current.Handler.MauiContext.Services.GetRequiredService<IMessenger>().Send<ShouldInitializeStatus>();
        }
    }
}
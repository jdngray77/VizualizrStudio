using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Services;
using Services.Messages;
using Services.Utility;
using ViewModels.FileBrowser;

namespace ViewModels
{
    [ObservableObject]
    public partial class MainPageViewModel
    {
        private StatusService statusService;

        public MainPageViewModel(
            FileBrowserViewModel fileBrowserViewModel, 
            IMessenger messenger,
            StatusService statusService)
        {
            fileBrowser = fileBrowserViewModel;
            this.statusService = statusService;

            messenger.Register<StatusMessage>(this, StatusMessageReceived);
            messenger.Register<TrackSelectedMessage>(this, SongSelected);
        }

        private async void SongSelected(object recipient, TrackSelectedMessage message)
        {
            if (!playerA.IsPlaying)
            {
                await playerA.Load(message.Value).ConfigureAwait(false);
                statusService.SetStatus(Minor: $"Deck A loaded '{message.Value.Name}'");
            }

            if (!playerB.IsPlaying)
            {
                await playerB.Load(message.Value).ConfigureAwait(false);
                statusService.SetStatus(Minor: $"Deck B loaded '{message.Value.Name}'");
            }
        }

        [ObservableProperty] 
        private PlayerViewModel playerA = new PlayerViewModel();

        // TODO mixer
        
        [ObservableProperty] 
        private PlayerViewModel playerB = new PlayerViewModel();

        [ObservableProperty]
        private FileBrowserViewModel fileBrowser;


        [ObservableProperty]
        private string statusMajor;

        [ObservableProperty]
        private string statusMinor;

        [ObservableProperty]
        public bool highlightMajor;

        [ObservableProperty]
        public bool highlightMinor;

        private void StatusMessageReceived(object recipient, StatusMessage message)
        {
            if (!String.IsNullOrEmpty(message.Major))
            {
                StatusMajor = message.Major;
                HighlightMajor = message.HighlightMajor;
                return;
            }

            if (!String.IsNullOrEmpty(message.Minor))
            {
                StatusMinor = message.Minor;
                HighlightMinor = message.HighlightMinor;
                return;
            }

            StatusMajor = null;
            StatusMinor = null;
            HighlightMinor = false;
            HighlightMajor = false;
        }
    }
}
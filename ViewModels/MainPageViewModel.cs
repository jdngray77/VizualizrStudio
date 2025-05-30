using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Services;
using Services.Audio;
using Services.Messages;
using Services.Utility;
using ViewModels.FileBrowser;

namespace ViewModels
{
    [ObservableObject]
    public partial class MainPageViewModel
    {
        private StatusService statusService;
        private readonly AudioHypervisor AudioHypervisor;

        public MainPageViewModel(
            FileBrowserViewModel fileBrowserViewModel,
            TitleBarViewModel titleBarViewModel,
            IMessenger messenger,
            StatusService statusService,
            AudioHypervisor audioHypervisor)
        {
            fileBrowser = fileBrowserViewModel;
            this.statusService = statusService;
            this.AudioHypervisor = audioHypervisor;

            messenger.Register<StatusMessage>(this, StatusMessageReceived);
            messenger.Register<TrackSelectedMessage>(this, SongSelected);

            this.TitleBar = titleBarViewModel;
            this.PlayerA = new PlayerViewModel(audioHypervisor);
            this.PlayerB = new PlayerViewModel(audioHypervisor);
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
        private TitleBarViewModel titleBar;

        [ObservableProperty] 
        private PlayerViewModel playerA;

        // TODO mixer

        [ObservableProperty]
        private PlayerViewModel playerB;

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
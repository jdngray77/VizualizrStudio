using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ServiceInterfaces;
using Vizualizr.Backend;
using Vizualizr.Backend.Audio;
using ViewModels.FileBrowser;
using Vizualizr.Backend.Audio.Player;
using Vizualizr.Backend.Messaging;

namespace ViewModels
{
    [ObservableObject]
    public partial class MainPageViewModel
    {
        private StatusService statusService;
        private readonly AudioHypervisor AudioHypervisor;
        private readonly INonCommonServices services;
        private readonly TrackHypervisor TrackHypervisor;

        public MainPageViewModel(
            FileBrowserViewModel fileBrowserViewModel,
            TitleBarViewModel titleBarViewModel,
            IMessenger messenger,
            StatusService statusService,
            AudioHypervisor audioHypervisor,
            INonCommonServices services,
            TrackHypervisor trackHypervisor,
            DeckManager deckManager)
        {
            fileBrowser = fileBrowserViewModel;
            this.statusService = statusService;
            this.AudioHypervisor = audioHypervisor;
            this.services = services;
            this.TrackHypervisor = trackHypervisor;

            messenger.Register<StatusMessage>(this, StatusMessageReceived);
            messenger.Register<TrackSelectedMessage>(this, SongSelected);
            messenger.Register<FrequentBackgroundUpdate>(this, this.FrequentBackgroundUpdate);

            this.TitleBar = titleBarViewModel;
            this.PlayerA = new PlayerViewModel(audioHypervisor, trackHypervisor, deckManager, statusService);
            this.PlayerB = new PlayerViewModel(audioHypervisor, trackHypervisor, deckManager, statusService);
        }

        private void SongSelected(object recipient, TrackSelectedMessage message)
        {
            TrackHypervisor.LoadToDeckAsync(message.Value);
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

        [ObservableProperty]
        public float cpuUsage;

        [ObservableProperty]
        public float gpuUsage;

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

        private void FrequentBackgroundUpdate(object recipient, FrequentBackgroundUpdate message)
        {
            CpuUsage = services.GetCpuUsage();
            GpuUsage = services.GetGpuUsage();
        }
    }
}
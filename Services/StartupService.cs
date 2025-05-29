using Services.Audio;

namespace Services
{
    public class StartupService
    {
        private readonly AudioHypervisor audioHypervisor;
        private readonly TrackManager trackManager;

        public StartupService(AudioHypervisor audioHypervisor, TrackManager trackManager)
        {
            this.audioHypervisor = audioHypervisor;
            this.trackManager = trackManager;
        }

        public async Task InitializeAsync()
        {
            audioHypervisor.TryInitialize();
            audioHypervisor.TrySelectDefaultOutputDevice();
        }
    }
}
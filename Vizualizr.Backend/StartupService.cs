using CommunityToolkit.Mvvm.Messaging;
using Vizualizr.Backend.Audio;
using Vizualizr.Backend.Messaging;

namespace Vizualizr.Backend
{
    public class StartupService
    {
        private readonly AudioHypervisor audioHypervisor;
        private readonly TrackHypervisor trackHypervisor;
        private readonly IMessenger messenger;

        private PeriodicTimer minuteTimer;
        private PeriodicTimer frequentTimer;


        public StartupService(
            AudioHypervisor audioHypervisor, 
            TrackHypervisor trackHypervisor,
            IMessenger messenger)
        {
            this.audioHypervisor = audioHypervisor;
            this.trackHypervisor = trackHypervisor;
            this.messenger = messenger;
        }

        public async Task InitializeAsync()
        {
            audioHypervisor.TrySelectDefaultOutputDevice();

            minuteTimer = new PeriodicTimer(TimeSpan.FromMinutes(1));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                do
                {
                    try
                    {
                        messenger.Send<BackgroundUpdate>();
                    }
                    catch (Exception ex) { }

                    await minuteTimer.WaitForNextTickAsync().ConfigureAwait(false);

                } while (true);
            });

            frequentTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            Task.Run(async () =>
            {
                do
                {
                    try
                    {
                        messenger.Send<FrequentBackgroundUpdate>();
                    }
                    catch (Exception ex) { }

                    await frequentTimer.WaitForNextTickAsync().ConfigureAwait(false);
                } while (true);
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
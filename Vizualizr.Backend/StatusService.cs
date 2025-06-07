using CommunityToolkit.Mvvm.Messaging;
using Vizualizr.Backend.Messaging;
using Vizualizr.Backend.Utility;

namespace Vizualizr.Backend
{
    public class StatusService
    {
        private readonly IMessenger messenger;
        private readonly ResettableTimer timer = new ResettableTimer(6000);

        public StatusService(IMessenger messenger)
        {
            this.messenger = messenger;
            timer.TimerElapsedWithoutReset += StatusTimeout;
        }

        private void StatusTimeout()
        {
            ImplSetStatus(string.Empty, string.Empty);
        }

        public void SetStatus(
            string? Minor = null,
            string? Major = null,
            bool highlightMajor = false,
            bool highlightMinor = false)
        {
            if (!timer.IsRunning)
            {
                timer.Start();
            }
            else 
            {
                timer.Reset();
            }

            if (Minor == null && Major == null)
            {
                return;
            }

            ImplSetStatus(Minor, Major, highlightMajor, highlightMinor);
        }

        private void ImplSetStatus(
            string? Minor = null,
            string? Major = null,
            bool highlightMajor = false,
            bool highlightMinor = false)
        {
            messenger.Send(new StatusMessage(Major, Minor, highlightMajor, highlightMinor));
        }
    }
}
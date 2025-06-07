using CommunityToolkit.Mvvm.Messaging.Messages;
using Vizualizr.Backend.Audio;

namespace Vizualizr.Backend.Messaging
{
    /**
     * Indicates that systems and services of the program should
     * begin loading.
     */
    public class ShouldInitializeStatus;

    /**
     * Indicates that non-frequent background tasks may update.
     * 
     * May only update once per minute or so.
     */
    public class BackgroundUpdate;

    /**
     * Indicates more frequent background tasks may update.
     * 
     * May only update every 5-10 seconds.
     */
    public class FrequentBackgroundUpdate;


    public class TrackSelectedMessage : ValueChangedMessage<TrackMetadata>
    {
        public TrackSelectedMessage(TrackMetadata value) : base(value)
        {
        }
    }

    public class StatusMessage
    {
        public StatusMessage(
            string? major = null, 
            string? minor = null,
            bool highlightMajor = false,
            bool highlightMinor = false)
        {
            Major = major;
            Minor = minor;
            HighlightMajor = highlightMajor;
            HighlightMinor = highlightMinor;
        }

        public bool HighlightMajor { get; init; }
        public bool HighlightMinor { get; init; }


        public string? Major { get; init; }
        public string? Minor { get; init; }
    }

}
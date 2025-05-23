using CommunityToolkit.Mvvm.Messaging.Messages;
using ViewModels;

namespace Services.Messages
{
    /**
     * Indicates that systems and services of the program should
     * begin loading.
     */
    public class ShouldInitializeStatus;

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
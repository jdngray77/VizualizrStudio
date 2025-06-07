namespace Vizualizr.Backend.Audio.Player
{
    /// <summary>
    /// Information regarding the current playback position of a player.
    /// </summary>
    public class Playhead
    {
        private double playbackPositionSampleIndex = 0;
        private double playbackSpeed = 1.0;

        private readonly int sampleRate;
        private readonly int sampleCount;

        public Playhead(int sampleRate, int sampleCount)
        {
            this.sampleRate = sampleRate > 0 ? sampleRate : throw new ArgumentOutOfRangeException(nameof(sampleRate));
            this.sampleCount = sampleCount > 0 ? sampleCount : throw new ArgumentOutOfRangeException(nameof(sampleCount));
        }

        /// <summary>
        /// If true, advancing from the end of the track will wrap to the start, and vice versa.
        /// </summary>
        public bool Wrap { get; set; } = false;

        /// <summary>
        /// Gets or sets the index of the audio sample the playhead is pointing at.
        /// Updates PlaybackTime when set.
        /// </summary>
        public double PlaybackPositionSampleIndex
        {
            get => playbackPositionSampleIndex;
            set
            {
                playbackPositionSampleIndex = value;
                PlaybackTime = playbackPositionSampleIndex / sampleRate;
            }
        }

        /// <summary>
        /// Gets or sets the playback position as a percentage of the total track length.
        /// </summary>
        public double PlaybackPercentage
        {
            get => playbackPositionSampleIndex / sampleCount;
            set => PlaybackPositionSampleIndex = value * sampleCount;
        }

        /// <summary>
        /// The playback time in seconds corresponding to the current sample index.
        /// </summary>
        public double PlaybackTime { get; private set; } = 0;

        /// <summary>
        /// Playback speed multiplier (e.g. 1.0 = normal speed, -1.0 = reverse).
        /// </summary>
        public double PlaybackSpeed
        {
            get => playbackSpeed;
            set => playbackSpeed = value;
        }

        /// <summary>
        /// Returns true when Wrap is false and the playhead has reached or passed the end.
        /// </summary>
        public bool Complete => !Wrap && playbackPositionSampleIndex >= sampleCount;

        /// <summary>
        /// Advances the playhead by PlaybackSpeed, wrapping or clamping as needed.
        /// </summary>
        public void Advance()
        {
            PlaybackPositionSampleIndex += playbackSpeed;

            if (Wrap)
            {
                if (playbackPositionSampleIndex >= sampleCount)
                    PlaybackPositionSampleIndex -= sampleCount;
                else if (playbackPositionSampleIndex < 0)
                    PlaybackPositionSampleIndex += sampleCount;
            }
            else
            {
                if (playbackPositionSampleIndex >= sampleCount)
                    PlaybackPositionSampleIndex = sampleCount;
                else if (playbackPositionSampleIndex < 0)
                    PlaybackPositionSampleIndex = 0;
            }
        }
    }
}

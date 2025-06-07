namespace Vizualizr.Backend.Audio
{
    public class Track
    {
        /// <summary>
        /// Metadata as obtained from the file on disk.
        /// </summary>
        public TrackMetadata Metadata { get; set; }
        
        /// <summary>
        /// Information about the Track's beat-grid.
        /// </summary>
        public BeatInfo BeatInfo { get; set; }

        /// <summary>
        /// Raw audio samples.
        /// </summary>
        public float[] Samples { get; set; }
        
        /// <summary>
        /// The quantity of audio samples this track contains.
        /// </summary>
        public int SampleCount => Samples?.Length ?? 0;

        /// <summary>
        /// Number of channels that can be found in <a cref="Samples"/>
        /// </summary>
        public int Channels { get; set; }

        /// <summary>
        /// The sample rate the raw samples should be played back at.
        /// </summary>
        public int SampleRate { get; set; }
        
        /// <summary>
        /// Total number of samples required to be fed to a stream in order to satisfy all channels.
        /// </summary>
        public int ChannelAdjustedSampleRate => SampleRate * Channels;
        
        /// <summary>
        /// Calculated length of the track.
        /// </summary>
        public TimeSpan Length { get; set; }
    }
}

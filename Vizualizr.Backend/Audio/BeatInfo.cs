namespace Vizualizr.Backend.Audio
{
    public class BeatInfo
    {
        /// <summary>
        /// The index, within the audio samples, at which the first beat can be found.
        /// </summary>
        public int IndexOfFirstBeat { get; set; }
        
        /// <summary>
        /// The beats per minute of the track.
        /// </summary>
        public float Bpm { get; set; }
    }
}
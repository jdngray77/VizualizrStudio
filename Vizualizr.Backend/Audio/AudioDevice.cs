using PortAudioSharp;

namespace Vizualizr.Backend.Audio
{
    /// <summary>
    /// Wrapper for pointers at an audio device in PortAudio.
    /// </summary>
    public class AudioDevice
    {
        public AudioDevice(int index, DeviceInfo device)
        {
            Index = index;
            Device = device;
        }
        
        /// <summary>
        /// The device index in PortAudio.
        ///
        /// Used to get enumerated device info from PortAudio by device index.
        /// </summary>
        public int Index { get; private init; }
        
        /// <summary>
        /// Port Audio's device API.
        /// </summary>
        public DeviceInfo Device { get; private init; }
    }
}
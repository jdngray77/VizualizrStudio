using System.Collections.Immutable;
using System.Diagnostics;
using PortAudioSharp;

namespace Vizualizr.Backend.Audio
{
    /// <summary>
    /// Singleton access hypervisor for PortAudio.
    /// 
    /// Manager, creator and owner of all audio renderers.
    ///
    /// Initializes and terminates PortAudio.
    /// </summary>
    public class AudioHypervisor : IDisposable
    {
        private AudioDevice? desiredAudioDevice = null;
        private readonly IList<AudioRenderer> renderers = new List<AudioRenderer>();
        private bool isDisposed = false;

        public AudioHypervisor()
        {
            PortAudio.Initialize();
        }

        /// <summary>
        /// Enumerates and polls for information about all connected audio devices.
        /// </summary>
        /// <returns></returns>
        public IList<AudioDevice> GetAudioDevices()
        {
            // TODO this likely needs initialization.
            
            int deviceCount = PortAudio.DeviceCount;

            IList<AudioDevice> devices = new List<AudioDevice>();
            
            for (int index = 0; index < deviceCount; index++)
            {
                var deviceInfo = PortAudio.GetDeviceInfo(index);
                devices.Add(new AudioDevice(index, deviceInfo));
            }

            return devices;
        }

        /// <summary>
        /// Enumerates all audio devices that support audio output.
        /// </summary>
        /// <returns></returns>
        public IList<AudioDevice> GetAudioOutputDevices()
        {
            // TODO this likely needs initialization.
            return GetAudioDevices().Where(it => it.Device.maxOutputChannels > 0).ToList();
        }

        /// <summary>
        /// Sets the output device of this hypervisor to the system default audio device.
        /// </summary>
        /// <returns></returns>
        public bool TrySelectDefaultOutputDevice()
        {
            var index = PortAudio.DefaultOutputDevice;

            if (index == -1)
            {
                Debug.WriteLine("PortAudio did not have a default output device to select.");
                return false;
            }
            
            var deviceInfo = PortAudio.GetDeviceInfo(index);

            return TrySelectOutputDevice(new AudioDevice(index, deviceInfo));
        }

        /// <summary>
        /// Attempts to select the provided audio device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool TrySelectOutputDevice(AudioDevice device)
        {
            desiredAudioDevice = device;
            return true;
        }

        public bool TrySelectDefaultOutputDeviceIfNoDevice()
        {
            if (desiredAudioDevice != null)
            {
                return true;
            }
            
            Debug.WriteLine("No device selected, attempting to choose default audio device.");
            
            return TrySelectDefaultOutputDevice();
        }

        /// <summary>
        /// Creates an audio renderer for the desired audio device.
        /// </summary>
        /// <returns></returns>
        public AudioRenderer CreateAudioRenderer()
        {
            TrySelectDefaultOutputDeviceIfNoDevice();

            if (desiredAudioDevice == null)
            {
                throw new Exception("Attempted to create a new audio renderer with no audio device selected.");
            }
            
            AudioRenderer audioRenderer = new AudioRenderer(desiredAudioDevice);
            
            WithRenderers(r => r.Add(audioRenderer));
            return audioRenderer;
        }

        /// <summary>
        /// Dismantles the provided renderer.
        /// </summary>
        /// <param name="renderer"></param>
        public void DestroyAudioRenderer(AudioRenderer renderer)
        {
            WithRenderers(r =>
            {
                // ReSharper disable once AccessToDisposedClosure
                r.Remove(renderer);
            });
            
            renderer.StopPlayback();
            renderer.Dispose();
        }
        
        /// <summary>
        /// Returns an immutable array of all current audio renderers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AudioRenderer> GetAudioRenderers()
        {
            IList<AudioRenderer> rends = null;
            WithRenderers(r =>
            {
                rends = renderers.ToImmutableArray();
            });

            return rends;
        }

        private void WithRenderers(Action<IList<AudioRenderer>> action)
        {
            lock (renderers)
            {
                action(renderers);
            }
        }
        
        /// <summary>
        /// Destroys all renders owned by this hypervisor.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            WithRenderers(r =>
            {
                foreach (var audioRenderer in renderers)
                {
                    audioRenderer.Dispose();
                }
            });

            PortAudio.Terminate();
            isDisposed = true;
        }
    }
}
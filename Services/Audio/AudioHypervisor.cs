 using System.Collections.Immutable;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using PortAudioSharp;

namespace Services.Audio
{
    public class AudioHypervisor : IDisposable
    {
        private IList<AudioRenderer> renderers = new List<AudioRenderer>();
        private bool isInitialized = false;
        private bool isDisposed = false;

        private (int index, DeviceInfo device)? desiredAudioDevice = null;
        
        public AudioHypervisor()
        {
        }

        public bool TryInitialize()
        {
            if (isInitialized)
            {
                return false;
            }

            PortAudio.Initialize();
            isInitialized = true;
            return true;
        }

        /**
         * Enumerates and polls for information about all connected audio devices.
         */
        public IList<DeviceInfo> GetAudioDevices()
        {
            int deviceCount = PortAudio.DeviceCount;

            IList<DeviceInfo> devices = new List<DeviceInfo>();
            
            for (int i = 0; i < deviceCount; i++)
            {
                devices.Add(PortAudio.GetDeviceInfo(i));                
            }

            return devices;
        }

        /**
         * Enumerates all audio devices that support audio out.
         */
        public IList<(int index, DeviceInfo info)> GetAudioOutputDevices()
        {
            var x = GetAudioDevices().Where(it => it.maxOutputChannels > 0).ToList();
            return x.Select(it => (x.IndexOf(it), it)).ToList();
        }

        public bool TrySelectDefaultOutputDevice()
        {
            if (!isInitialized)
            {
                return false;
            }

            var index = PortAudio.DefaultOutputDevice;

            if (index == -1)
            {
                Debug.WriteLine("PortAudio did not have a default output device to select.");
                return false;
            }
            
            var deviceInfo = PortAudio.GetDeviceInfo(index);

            return TrySelectOutputDevice((index, deviceInfo));
        }

        public bool TrySelectOutputDevice((int, DeviceInfo) device)
        {
            if (!isInitialized)
            {
                return false;
            }

            desiredAudioDevice = device;
            return true;
        }

        public AudioRenderer CreateAudioRenderer()
        {
            AssertInitialized();

            if (desiredAudioDevice == null)
            {
                Debug.WriteLine("No device selected, choosing default audio device.");
                var index = PortAudio.DefaultOutputDevice;
                var info = PortAudio.GetDeviceInfo(index);
                desiredAudioDevice = (index, info);
            }
        
            AudioRenderer audioRenderer = new AudioRenderer(desiredAudioDevice.Value);
            renderers.Add(audioRenderer);
            return audioRenderer;
        }

        public void DestroyAudioRenderer(AudioRenderer renderer)
        {
            AssertInitialized();

            lock (renderers)
            {
                if (renderers.Contains(renderer))
                {
                    renderers.Remove(renderer);
                }
            }
            
            renderer.StopPlayback();
            renderer.Dispose();
        }

        /**
         * Returns an immutable array of all current audio renderers.
         */
        public IEnumerable<AudioRenderer> GetAudioRenderers()
        {
            lock (renderers)
            {
                return renderers.ToImmutableArray();
            }
        }

        private void AssertNotInitialized()
        {
            if (isInitialized)
            {
                throw new Exception("Port audio initialized. This operation cannot be performed post-initialization.");
            }
        }
        
        private void AssertInitialized()
        {
            if (!isInitialized)
            {
                throw new Exception("Port audio not initialized. This operation cannot be performed unless initialzied.");
            }
        }
        
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            lock (renderers)
            {
                foreach (var audioRenderer in renderers)
                {
                    audioRenderer.Dispose();
                }
            }

            isDisposed = true;
        }
    }
}
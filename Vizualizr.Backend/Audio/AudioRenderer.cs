using PortAudioSharp;
using Stream = PortAudioSharp.Stream;

namespace Vizualizr.Backend.Audio
{
    public delegate StreamCallbackResult SampleProvider(uint bufferSize, IntPtr output);

    /// <summary>
    /// A sample audio renderer.
    ///
    /// Must be created via, and owned by, <see cref="AudioHypervisor"/>
    /// </summary>
    public class AudioRenderer : IDisposable
    {
        public const uint DEFAULT_BUFFER_SIZE = 256;
        
        private readonly AudioDevice targetAudioDevice;
        private Stream? audioStream = null;
        private bool isDisposed = false;
        
        private StreamParameters outputStreamParameters;
        private SampleProvider sampleProvider = null;
        private uint bufferSize = DEFAULT_BUFFER_SIZE;

        public bool IsPlaying => IsConfigured && audioStream is { IsActive: true };

        public bool IsConfigured => audioStream != null;

        public AudioRenderer(AudioDevice targetAudioDevice)
        {
            this.targetAudioDevice = targetAudioDevice;
        }

        /// <summary>
        /// Configures an audio stream ready for audio of the specified shape.
        /// </summary>
        /// <param name="sampleProvider">
        /// An unsafe function which is called very regularly to fill the next audio buffer.
        ///
        /// A lot of trust is placed in this function.
        ///
        /// It:
        ///  - should take as minimal time as possible.
        ///    if the buffer is not populated before it runs before the stream catches up with the buffer
        ///    then the audio will stutter.
        /// 
        ///  - is provided a pointer to a buffer of a pre-determined size, according to the buffer size * channel count.
        ///  - is expected to populate that buffer with audio samples.
        ///  - must return a result which will control the state of the audio stream.
        ///
        /// </param>
        public void Configure(
            int channels,
            int sampleRate,
            SampleProvider sampleProvider,
            
            // How much data we provide to the audio stream with each sample.
            // Higher provides longer length of audio data, but at cost of increased latency,
            // as the audio library will call back to this class less frequently for new data.
            // However, higher makes the stream more robust - in that the stream can continue to play
            // if the application is too busy to respond quick enough to the callback, which would lead
            // to stuttering in playback.
            uint bufferSize = DEFAULT_BUFFER_SIZE)
        {
            if (IsConfigured)
            {
                Close();
            }
            
            this.bufferSize = bufferSize;
            
            outputStreamParameters = new StreamParameters
            {
                device = targetAudioDevice.Index,
                channelCount = channels != 0 ? Math.Min(targetAudioDevice.Device.maxOutputChannels, channels) : targetAudioDevice.Device.maxOutputChannels,
                sampleFormat = SampleFormat.Float32,
                suggestedLatency = PortAudio.GetDeviceInfo(PortAudio.DefaultOutputDevice).defaultLowOutputLatency
            };
            
            audioStream = new Stream(
                null,
                outputStreamParameters,
                sampleRate * channels,
                bufferSize,
                StreamFlags.NoFlag,
                Callback,
                null);
            
            this.sampleProvider = sampleProvider;
        }

        /**
         * Dumps the audio stream,
         * forgets all playback configuration.
         *
         * Prepares this renderer for re-use with another configuration.
         */
        public void Close()
        {
            if (!IsConfigured)
            {
                return;
            }

            if (audioStream is not { IsStopped: true })
            {
                audioStream!.Stop();
            }
            
            audioStream?.Close();
            audioStream?.Dispose();
            audioStream = null;
            GC.Collect();
        }
        
        public void StartPlayback()
        {
            if (audioStream?.IsActive == true)
            {
                return;
            }

            audioStream?.Start();
        }

        public void StopPlayback()
        {
            if (audioStream?.IsStopped == true)
            {
                return;
            }

            audioStream?.Stop();
        }

        private unsafe StreamCallbackResult Callback(
            IntPtr input,
            IntPtr output,
            uint frameCount,
            ref StreamCallbackTimeInfo timeInfo,
            StreamCallbackFlags statusFlags,
            IntPtr userDataPtr)
        {
            if (!IsConfigured)
            {
                return StreamCallbackResult.Abort;
            }

            return sampleProvider(bufferSize, output);
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            Close();
            isDisposed = true;
        }
    }
}
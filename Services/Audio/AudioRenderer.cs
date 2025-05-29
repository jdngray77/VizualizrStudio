using PortAudioSharp;
using Stream = PortAudioSharp.Stream;

namespace Services.Audio
{
    /**
    * An instantiable audio renderer, using PortAudio.
    */
    public class AudioRenderer : IDisposable
    {
        private readonly (int index, DeviceInfo device) targetAudioDevice;
        private Stream? audioStream = null;
        private bool isDisposed = false;

        /**
         * Occours every time the audio stream accepts a new set
         * of sample data from this renderer.
         *
         * Use to update playback information, i.e play position.
         */
        public event EventHandler<double> sampleOccurred;

        #region Audio State

        private bool hasAudioData = false;
        private float[]? samples;
        private int sampleRate = 44100;
        private uint framesPerBuffer = 256;
        private double playbackPosition = 0;
        private double playbackSpeed = 1.0;
        private StreamParameters outputStreamParameters;

        #endregion

        public double PlaybackPercentage
        {
            get
            {
                return playbackPosition / samples.Length;
            }

            set
            {
                playbackPosition = value * sampleRate;
            }
        }

        public bool IsPlaying => hasAudioData && audioStream != null && audioStream.IsActive;

        public bool IsLoaded => hasAudioData && audioStream != null;

        public double PlaybackTime { get; private set; } = 0D;

        public AudioRenderer((int index, DeviceInfo device) targetAudioDevice)
        {
            this.targetAudioDevice = targetAudioDevice;
        }

        public void LoadSamples(
            int sampleRate = 44100, 
            uint framesPerBuffer = 256, 
            int? channels = null,
            params float[] samples)
        {
            if (hasAudioData)
            {
                throw new Exception("Audio renderer already contains data; unload first.");
            }
            
            outputStreamParameters = new StreamParameters
            {
                device = targetAudioDevice.index,
                channelCount = channels != null ? Math.Min(targetAudioDevice.device.maxOutputChannels, channels.Value) : targetAudioDevice.device.maxOutputChannels,
                sampleFormat = SampleFormat.Float32,
                suggestedLatency = PortAudio.GetDeviceInfo(PortAudio.DefaultOutputDevice).defaultLowOutputLatency
            };
            
            this.samples = samples;
            this.framesPerBuffer = framesPerBuffer;
            this.sampleRate = sampleRate * outputStreamParameters.channelCount;
            
            audioStream = new Stream(
                null,
                outputStreamParameters,
                this.sampleRate,
                this.framesPerBuffer,
                StreamFlags.NoFlag,
                Callback,
                null);

            hasAudioData = true;
        }

        /**
         * Dumps the audio stream,
         * cleans all samples from memory,
         * forgets all playback configuration.
         *
         * Prepares this renderer for re-use with another sample set.
         */
        public void Unload()
        {
            if (!hasAudioData)
            {
                return;
            }

            if (audioStream != null && !audioStream.IsStopped)
            {
                audioStream.Stop();
            }
            
            audioStream?.Close();
            audioStream?.Dispose();
            audioStream = null;
            playbackPosition = 0;

            samples = null;
            hasAudioData = false;
            
            GC.Collect();
        }
        
        public void StartPlayback()
        {
            if (!hasAudioData || audioStream?.IsActive == true)
            {
                return;
            }

            audioStream?.Start();
        }

        public void StopPlayback()
        {
            if (!hasAudioData || audioStream?.IsStopped == true)
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
            if (!hasAudioData || samples == null || samples?.Length == 0)
            {
                return StreamCallbackResult.Abort;
            }

            PlaybackTime = playbackPosition / sampleRate;
            
            float* outBuffer = (float*)output;
            int sampleCount = (int)frameCount / 2; // TODO need to update this to correct channel count.

            for (int i = 0; i < frameCount; i++)
            {
                // Calculate integer and fractional part of playback position
                int posInt = Math.Min((int)playbackPosition, samples!.Length - 1);
                float frac = (float)(playbackPosition - posInt);

                // Boundary check & wrap-around
                // TODO don't wrap, signal complete.
                int nextPos = (posInt + 1) % samples!.Length;

                // Linear interpolation between samples
                float sample = (1 - frac) * samples[posInt] + frac * samples[nextPos];

                // Apply simple lowpass effect
                // float filteredSample = 0.5f * sample + 0.5f * lastFilteredSample;
                // lastFilteredSample = filteredSample;

                // Write stereo output (same on left and right)
                // TODO multi-channels.
                outBuffer[i * 2] = sample;
                outBuffer[i * 2 + 1] = sample;

                // Update playback position by speed (scratching changes this)
                playbackPosition += playbackSpeed;

                // Wrap playback position
                //if (playbackPosition >= samples.Length) 
                //    playbackPosition -= samples.Length;
                //else if (playbackPosition < 0) playbackPosition += samples.Length;

                if (playbackPosition >= samples.Length || playbackPosition < 0)
                {
                    return StreamCallbackResult.Complete;
                }
            }

            Task.Run(() =>
            {
                sampleOccurred?.Invoke(this, playbackPosition);
            });

            return 0;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            Unload();
            isDisposed = true;
        }
    }
}
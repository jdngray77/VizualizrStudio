using SoundTouch;

namespace Vizualizr.Backend.Audio
{
    /// <summary>
    /// Static utility functions for analysis of audio samples.
    /// </summary>
    public static class AudioUtilities
    {
        /// <summary>
        /// Returns the index of the first sample which is >= threshold.
        /// </summary>
        public static int DetermineLeadInEndIndex(float[] samples, float threshold = 0.0001F)
        {
            return Array.FindIndex(samples, x => x >= threshold);
        }
        
        /// <summary>
        /// Estimates if the sample index provided falls on the beat grid,
        /// based on a known starting beat and the BPM.
        /// </summary>
        /// <param name="threshold">If the sample falls between the nearest best +/- the threshold it will be considered a beat.</param>
        /// <returns></returns>
        public static bool IsBeat(BeatInfo info, int sampleIndex, int threshold = 100)
        {
            if (info == null || sampleIndex < info.IndexOfFirstBeat)
            {
                return false;
            }

            int samplesSinceFirstBeat = sampleIndex - info.IndexOfFirstBeat;
            int beatNumber = (int)Math.Round(samplesSinceFirstBeat / (double)info.IndexOfFirstBeat);
            int closestBeatSample = info.IndexOfFirstBeat + beatNumber * info.IndexOfFirstBeat;

            int diff = Math.Abs(sampleIndex - closestBeatSample);
            return diff <= threshold;
        }
        
        /// <summary>
        /// Uses SoundTouch to analyze the track's BPM, and <see cref="DetectFirstBeat"/>
        /// to estimate the sample index of the first beat.
        /// </summary>
        public static async Task<BeatInfo> AnalyzeBeatAsync(float[] samples, int channelCount, int sampleRate)
        {
            ArgumentNullException.ThrowIfNull(samples);
            ArgumentOutOfRangeException.ThrowIfLessThan(channelCount, 1);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);

            var bpmDetector = new BpmDetect(channelCount, sampleRate);
            
            var bpm = await Task.Run(() =>
            {
                int chunkSize = 2048 * channelCount;
                for (int i = 0; i < samples.Length; i += chunkSize)
                {
                    int length = Math.Min(chunkSize, samples.Length - i);
                    bpmDetector.InputSamples(samples.Skip(i).Take(length).ToArray(), length / 2);
                }

                return (int)Math.Round(bpmDetector.GetBpm());
            }).ConfigureAwait(false);
            
            return new BeatInfo
            {
                Bpm = bpm,
                IndexOfFirstBeat = DetectFirstBeat(sampleRate, bpm, samples),
            };
        }
        
        
        /// <summary>
        /// Estimates the sample index of the first beat.
        /// </summary>
        public static int DetectFirstBeat(int sampleRate, int bpm, float[] samples)
        {
            int beatIntervalSamples = (int)(sampleRate * 60.0 / bpm);
            int windowSize = 1024; 
            int searchRange = beatIntervalSamples * 4;

            int bestOffset = 0;
            double maxEnergy = 0;

            for (int offset = 0; offset < searchRange && offset + windowSize < samples.Length; offset += 32)
            {
                double energy = 0;
                for (int i = 0; i < windowSize; i++)
                {
                    float sample = samples[offset + i];
                    energy += sample * sample;
                }

                if (energy > maxEnergy)
                {
                    maxEnergy = energy;
                    bestOffset = offset;
                }
            }

            return bestOffset;
        }
        
        
        public static float[] DownmixToMono(float[] input, int numChannels)
        {
            int numFrames = input.Length / numChannels;
            float[] mono = new float[numFrames];

            for (int i = 0; i < numFrames; i++)
            {
                float sum = 0;
                for (int ch = 0; ch < numChannels; ch++)
                {
                    sum += input[i * numChannels + ch];
                }
                mono[i] = sum / numChannels; // average to mono
            }

            return mono;
        }
    }
}
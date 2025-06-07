using PortAudioSharp;

namespace Vizualizr.Backend.Audio.Player
{
    public class Deck
    {
        private AudioRenderer renderer;

        public Deck(AudioHypervisor hypervisor)
        {
            renderer = hypervisor.CreateAudioRenderer();
        }

        /// <summary>
        /// The track that this player currently has loaded, if any
        /// </summary>
        public Track? Track { get; private set; } = null;

        /// <summary>
        /// True if the player has a track.
        /// </summary>
        public bool TrackLoaded => Track != null;
        
        /// <summary>
        /// Information about the position of the virtual play head.
        /// Keeps track of our progress of playback.
        /// </summary>
        public Playhead Playhead { get; private set; }

        /// <summary>
        /// A un-controlled volume modifer applied to playback.
        ///
        /// Increases output volume, potentially above the maximum playback volume,
        /// but potentially at the cost of reduced audio quality.
        ///
        /// Has potential to induce audio clipping (i.e create audio which is too amplified to be rendered with accuracy.)
        /// </summary>
        public float Gain { get; set; } = 1f;

        public bool IsPlaying => renderer.IsPlaying;
        
        /// <summary>
        /// Invoked every time this player provides a new set of samples
        /// to the audio stream.
        ///
        /// Use to update playback information, i.e play position.
        ///
        /// Do not use for heavy operations; called extremely frequently.
        /// </summary>
        public event EventHandler<double>? AudioSampled;
        
        public event EventHandler<TrackMetadata>? TrackLoadBegan;
        
        public event EventHandler<Track>? TrackLoadCompleted;

        /// <summary>
        /// Called prior to LoadTrack to notify consumers
        /// of this deck that track loading has begun.
        ///
        /// Populates metadata and notifies views that loading
        /// of actual track data, and analysis, is in progress.
        /// </summary>
        /// <param name="metadata"></param>
        public void LoadTrackLite(TrackMetadata metadata)
        {
            if (TrackLoaded)
            {
                Unload();
            }
            
            TrackLoadBegan?.Invoke(this, metadata);
        }
        
        /// <summary>
        /// Called once samples have been loaded from disk.
        ///
        /// Minimum data here is:
        ///  - Samples
        ///  - Sample rate
        ///  - Channels
        ///  - Sample Count
        ///
        /// Beat-grid data does not need to be available at this time.
        /// </summary>
        /// <param name="track"></param>
        public void LoadTrack(Track track)
        {
            if (TrackLoaded)
            {
                Unload();
            }
            
            renderer.Configure(track.Channels, track.SampleRate, PopulateAudioBuffer);
            Playhead = new Playhead(track.SampleRate, track.SampleCount);
            Track = track;
            Cue();

            TrackLoadCompleted?.Invoke(this, track);
        }

        public void Unload()
        {
            if (!TrackLoaded)
            {
                return;
            }
            
            renderer.Close();
            Playhead = null;
            Track = null;
            GC.Collect();
        }

        public void Play()
        {
            if (!TrackLoaded || renderer.IsPlaying)
            {
                return;
            }
            
            renderer.StartPlayback();
        }

        public void Pause()
        {
            if (!TrackLoaded || renderer.IsPlaying)
            {
                return;
            }

            renderer.StopPlayback();
        }
        
        public void Stop()
        {
            if (!TrackLoaded || !renderer.IsPlaying)
            {
                return;
            }
            
            renderer.StopPlayback();
            Cue();
        }

        public void Cue()
        {
            if (!TrackLoaded)
            {
                return;
            }
            
            Playhead.PlaybackPositionSampleIndex = AudioUtilities.DetermineLeadInEndIndex(Track!.Samples);
        }

        /// <summary>
        /// Calibrates the beat-grid's first beat at the current playhead position.
        /// </summary>
        public void SetFirstBeat()
        {
            if (!TrackLoaded)
            {
                return;
            }
            
            Track.BeatInfo.IndexOfFirstBeat = (int)Playhead.PlaybackPositionSampleIndex;
        }
        
        /// <summary>
        /// Populates the audio buffer with the next chunk of audio.
        /// </summary>
        private unsafe StreamCallbackResult PopulateAudioBuffer(uint bufferSize, IntPtr outPtr)
        {
            if (!TrackLoaded)
            {
                return StreamCallbackResult.Abort;
            }
            
            float* outBuffer = (float*) outPtr;

            for (int sampleNumber = 0; sampleNumber < bufferSize; sampleNumber++)
            {
                // Calculate integer and fractional part of playback position
                int posInt = Math.Min((int)Playhead.PlaybackPositionSampleIndex, Track!.Samples!.Length - 1);
                float frac = (float)(Playhead.PlaybackPositionSampleIndex - posInt);

                // Boundary check & wrap-around
                int nextPos = (posInt + 1) % Track!.Samples!.Length;

                // Linear interpolation between samples
                float sample = (1 - frac) * Track!.Samples[posInt] + frac * Track!.Samples[nextPos];

                // Apply gain
                sample = sample * Gain;

                // Apply simple lowpass effect
                // float filteredSample = 0.5f * sample + 0.5f * lastFilteredSample;
                // lastFilteredSample = filteredSample;

                // Write stereo output (same on left and right)
                // TODO multi-channels.
                outBuffer[sampleNumber * 2] = sample;
                outBuffer[sampleNumber * 2 + 1] = sample;

                // Update playback position
                Playhead.Advance();
                
                if (Playhead.Complete)
                {
                    NotifySampled();
                    return StreamCallbackResult.Complete;
                }
            }

            NotifySampled();
            return StreamCallbackResult.Continue;
        }

        private void NotifySampled()
        {
            if (AudioSampled != null)
            {
                Task.Run(() =>
                {
                    AudioSampled?.Invoke(this, Playhead.PlaybackPositionSampleIndex);
                });
            }
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vizualizr.Backend;
using Vizualizr.Backend.Audio;
using Vizualizr.Backend.Audio.Player;

namespace ViewModels
{
    [ObservableObject]
    public partial class PlayerViewModel : IDisposable
    {
        // Audio playback
        private Deck deck;

        private readonly DeckManager deckManager;
        private readonly AudioHypervisor audioHypervisor;
        private readonly TrackHypervisor trackHypervisor;
        private readonly StatusService statusService;

        private bool userSeeking = false;

        public PlayerViewModel(
            AudioHypervisor audioHypervisor,
            TrackHypervisor trackHypervisor,
            DeckManager deckManager,
            StatusService status)
        {
            this.audioHypervisor = audioHypervisor;
            this.trackHypervisor = trackHypervisor;
            this.deckManager = deckManager;
            this.statusService = status;
            
            deck = deckManager.CreateDeck();
            
            deck.TrackLoadBegan += (sender, trackMetadata) =>
            {
                Metadata = trackMetadata;
            };
            
            deck.TrackLoadCompleted += (sender, track) =>
            {
                Samples = track.Samples;
                Bpm = track.BeatInfo?.Bpm ?? 0;
                SongTotalTime = track.Length;
                
                updatingProgress = true;
                ProgressPercentage = (float)deck.Playhead.PlaybackPercentage;
                updatingProgress = false;
            };
        }

        // Private state
        private volatile bool updatingProgress = false;

        // Vizualizr.Maui time lerping
        TimeSpan lastKnownTime = TimeSpan.Zero;
        DateTime lastUpdateTime = DateTime.Now;
        
        // public non-observable
        public bool IsPlaying => deck?.IsPlaying == true;
        
        [ObservableProperty]
        private float db = 0;

        [ObservableProperty]
        private float zoom = 0;

        [ObservableProperty]
        private float volume = .5f;

        [ObservableProperty]
        private float[] samples = [];

        [ObservableProperty]
        private TrackMetadata metadata = null   ;

        [ObservableProperty]
        private TimeSpan songCurrentTime = new TimeSpan(0);
        
        [ObservableProperty]
        private TimeSpan songTotalTime = new TimeSpan(0);
        
        [ObservableProperty]
        private float progressPercentage = 0f;

        [ObservableProperty]
        private float bpm;

        [RelayCommand]
        private void Cue()
        {
            deck.Cue();
            
            updatingProgress = true;
            ProgressPercentage = (float)deck.Playhead.PlaybackPercentage;
            updatingProgress = false;

            SongCurrentTime = TimeSpan.FromSeconds(deck.Playhead.PlaybackTime);
        }

        [RelayCommand]
        public void Play()
        {
            if (deck == null || !deck.TrackLoaded)
            {
                return;
            }

            deck.Play();
            
            Thread t = new Thread(() =>
            {
                lastKnownTime = TimeSpan.Zero;
                lastUpdateTime = DateTime.Now;

                while (deck?.IsPlaying == true) // TODO this will kill the thread.
                {
                    // TODO timecode caching does not handle stream wrapping.
                    var newKnownTime = TimeSpan.FromSeconds(deck.Playhead.PlaybackTime);
                    var now = DateTime.Now;
    
                    if (newKnownTime > lastKnownTime)
                    {
                        // New audio time detected, update cache
                        lastKnownTime = newKnownTime;
                        lastUpdateTime = now;
                    }
    
                    // Interpolate time between updates
                    TimeSpan interpolatedTime = lastKnownTime + (now - lastUpdateTime);
    
                    // Clamp so it does not go beyond TotalTime
                    if (interpolatedTime > SongTotalTime)
                        interpolatedTime = SongTotalTime;
    
                    // Calculate progress
                    double progress = deck.Playhead.PlaybackPercentage;

                    updatingProgress = true;
                    ProgressPercentage = (float)progress;
                    updatingProgress = false;

                    int index = (int)(progress * samples.Length);
                    int length = Math.Min(samples.Length - index, 10000);

                    float sum = 0;
                    for (int i = index; i < index + length; i++)
                    {
                        float sample = samples[i];
                        sum += sample * sample;
                    }

                    float rms = (float)Math.Sqrt(sum / length);
                    Db = (rms * 2) * volume;
                    Thread.Sleep(10); // 60fps update rate
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        [RelayCommand]
        public void Stop()
        {
            deck?.Stop();
        }

        [RelayCommand]
        private void UserStartedManualSeek()
        {
            userSeeking = true;

            // TODO realistically, we don't want to do this. Keep the stream open and manipulate the
            //      samples.
            //if (renderer?.IsPlaying == true)
            //{
            //    renderer.StopPlayback();
            //}
        }

        [RelayCommand]
        private void UserStoppedManualSeek()
        {
            userSeeking = false;

            // TODO only resume if was previously playing.
            //if (renderer?.IsPlaying == true)
            //{
            //    renderer.StartPlayback();
            //}
        }

        partial void OnProgressPercentageChanged(float value)
        {
            if (deck == null)
            {
                return;
            }

            if (!userSeeking)
            {
                SongCurrentTime = TimeSpan.FromSeconds(deck.Playhead.PlaybackTime);
            }

            if (updatingProgress)
            {
                return;
            }

            if (deck != null)
            {
                deck.Playhead.PlaybackPercentage = value;
                lastKnownTime = TimeSpan.Zero;
            }
        }

        partial void OnVolumeChanged(float value)
        {
            if (deck != null)
            {
                // TODO renderer volume.
                //renderer.Volume = value;
                deck.Gain = value;
            }
        }

        public void Dispose()
        {
            //deck?.Dispose();
        }
    }
}
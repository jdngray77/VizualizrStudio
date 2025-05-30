using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.Wave;
using System.Linq;
using Services.Audio;

namespace ViewModels
{
    [ObservableObject]
    public partial class PlayerViewModel : IDisposable
    {
        // Audio playback
        private AudioRenderer? renderer = null;

        private readonly AudioHypervisor audioHypervisor;

        public PlayerViewModel(AudioHypervisor audioHypervisor)
        {
            this.audioHypervisor = audioHypervisor;
        }

        // Private state
        private volatile bool updatingProgress = false;

        // Visualizer time lerping
        TimeSpan lastKnownTime = TimeSpan.Zero;
        DateTime lastUpdateTime = DateTime.Now;
        
        // public non-observable
        public bool IsPlaying => renderer?.IsPlaying == true;
        
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
        
        [RelayCommand]
        public async Task Load(TrackMetadata metadata)
        {
            if (renderer == null)
            {
                renderer = audioHypervisor.CreateAudioRenderer();
            }

            if (renderer.IsLoaded)
            {
                renderer.Unload();
            }

            string path = metadata.FilePath;

            if (!File.Exists(path))
            {
                return;
            }

            Metadata = metadata;

            await Task.Run(async () =>
            {
                // Dump existing data
                renderer.Unload();
                ProgressPercentage = 0f;

                // Create file stream
                // TODO this must be replaced as it's dependant on NAudio, which is windows-only.
                await using var audioReader = new AudioFileReader(path);

                var sampleReader = audioReader.ToStereo();
                // Load samples from disk
                var buffer = new float[(audioReader.Length * 2) / sizeof(float)];
                int read = sampleReader.Read(buffer, 0, buffer.Length);
                Samples = buffer.Take(read).ToArray();

                // Load into renderer
                renderer.LoadSamples(
                    sampleRate: audioReader.WaveFormat.SampleRate,
                    samples: Samples);
                
                SongTotalTime = audioReader.TotalTime;
            }).ConfigureAwait(false);
        }

        [RelayCommand]
        public void Play()
        {
            if (renderer == null || !renderer.IsLoaded)
            {
                return;
            }

            renderer.StartPlayback();
            
            Thread t = new Thread(() =>
            {
                lastKnownTime = TimeSpan.Zero;
                lastUpdateTime = DateTime.Now;

                while (renderer?.IsPlaying == true) // TODO this will kill the thread.
                {
                    // TODO timecode caching does not handle stream wrapping.
                    var newKnownTime = TimeSpan.FromSeconds(renderer.PlaybackTime); // TODO dubious.
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
                    double progress = renderer.PlaybackPercentage;

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
            renderer?.StopPlayback();
        }

        [RelayCommand]
        private void UserStartedManualSeek()
        {
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
            // TODO only resume if was previously playing.
            //if (renderer?.IsPlaying == true)
            //{
            //    renderer.StartPlayback();
            //}
        }

        partial void OnProgressPercentageChanged(float value)
        {
            if (renderer == null)
            {
                return;
            }

            SongCurrentTime = TimeSpan.FromSeconds(renderer.PlaybackTime);

            if (updatingProgress)
            {
                return;
            }

            if (renderer != null)
            {
                var newPos = value * SongTotalTime.TotalSeconds;
                // TODO manipulate playback time.
                // renderer.PlaybackTime = TimeSpan.FromSeconds(newPos);
                renderer.PlaybackPercentage = newPos;
                lastKnownTime = TimeSpan.Zero;
            }
        }

        partial void OnVolumeChanged(float value)
        {
            if (renderer != null)
            {
                // TODO renderer volume.
                //renderer.Volume = value;
                renderer.Gain = value;
            }
        }

        public void Dispose()
        {
            renderer?.Dispose();
        }
    }
}
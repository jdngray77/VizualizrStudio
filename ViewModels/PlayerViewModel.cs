using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.Wave;
using System.Linq;

namespace ViewModels
{
    [ObservableObject]
    public partial class PlayerViewModel
    {
        // Audio playback
        private AudioFileReader audioReader;
        private WaveOutEvent waveOut;

        // Private state
        private volatile bool updatingProgress = false;

        // Visualizer time lerping
        TimeSpan lastKnownTime = TimeSpan.Zero;
        DateTime lastUpdateTime = DateTime.Now;

        // public non-observable
        public bool IsPlaying => waveOut?.PlaybackState == PlaybackState.Playing;

        [ObservableProperty]
        private float db = 0;

        [ObservableProperty]
        private float zoom = 10;

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
            string path = metadata.FilePath;

            if (!File.Exists(path))
            {
                return;
            }

            Metadata = metadata;

            await Task.Run(async () =>
            {
                // Dump existing data
                if (audioReader != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = null;

                    await audioReader.DisposeAsync().ConfigureAwait(false);
                    audioReader = null;
                    GC.Collect();
                }

                ProgressPercentage = 0f;

                // Create file stream
                audioReader = new AudioFileReader(path);

                // Load samples
                var buffer = new float[audioReader.Length / sizeof(float)];
                int read = audioReader.Read(buffer, 0, buffer.Length);
                Samples = buffer.Take(read).ToArray();

                audioReader.Dispose();
                audioReader = new AudioFileReader(path);

                // Create basic audio player
                waveOut = new WaveOutEvent();
                waveOut.Init(audioReader);

                SongTotalTime = audioReader.TotalTime;
            }).ConfigureAwait(false);
        }

        [RelayCommand]
        public void Play()
        {
            if (waveOut == null)
            {
                return;
            }

            waveOut.Play();
            
            Thread t = new Thread(() =>
            {
                lastKnownTime = TimeSpan.Zero;
                lastUpdateTime = DateTime.Now;

                while (waveOut != null && waveOut.PlaybackState != PlaybackState.Stopped)
                {
                    if (waveOut.PlaybackState == PlaybackState.Paused)
                    {
                        Thread.Sleep(30);
                        continue;
                    }

                    var newKnownTime = audioReader.CurrentTime;
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
                    if (interpolatedTime > audioReader.TotalTime)
                        interpolatedTime = audioReader.TotalTime;
    
                    // Calculate progress
                    float progress = (float)(interpolatedTime.TotalSeconds / audioReader.TotalTime.TotalSeconds);

                    updatingProgress = true;
                    ProgressPercentage = progress;
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
                    Db = (rms * 2) * waveOut.Volume;

                    Thread.Sleep(10); // 60fps update rate
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        [RelayCommand]
        public void Stop()
        {
            waveOut.Stop();
        }

        [RelayCommand]
        private void UserStartedManualSeek()
        {
            if (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Pause();
            }
        }

        [RelayCommand]
        private void UserStoppedManualSeek()
        {
            if (waveOut != null && waveOut.PlaybackState == PlaybackState.Paused)
            {
                waveOut.Play();
            }
        }

        partial void OnProgressPercentageChanged(float value)
        {
            if (audioReader == null)
            {
                return;
            }

            SongCurrentTime = audioReader.CurrentTime;

            if (updatingProgress)
            {
                return;
            }

            if (waveOut != null && audioReader != null)
            {
                var newPos = value * audioReader.TotalTime.TotalSeconds;
                audioReader.CurrentTime = TimeSpan.FromSeconds(newPos);
                lastKnownTime = TimeSpan.Zero;
            }
        }

        partial void OnVolumeChanged(float value)
        {
            if (waveOut != null)
            {
                waveOut.Volume = value;
            }
        }

        public static float Median(float[] source)
        {
            if (source == null || source.Length == 0)
                throw new ArgumentException("Array is empty or null.");

            float[] sorted = source.OrderBy(x => x).ToArray();
            int count = sorted.Length;
            int mid = count / 2;

            if (count % 2 == 0)
            {
                // Even number of elements: average the two middle ones
                return (sorted[mid - 1] + sorted[mid]) / 2f;
            }
            else
            {
                // Odd number: return the middle one
                return sorted[mid];
            }
        }
    }
}
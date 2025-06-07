using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.ApplicationModel;
using Vizualizr.Backend;
using ViewModels.TreeView;
using System.Diagnostics;
using Vizualizr.Backend.Audio;
using Vizualizr.Backend.Messaging;

namespace ViewModels.FileBrowser
{
    [ObservableObject]
    public partial class FileBrowserViewModel
    { 
        private readonly string[] supportedExtensions = new[] { ".mp3", ".wav", ".flac", ".m4a" };
        
        private readonly TrackHypervisor trackHypervisor;
        private readonly StatusService statusService;
        private readonly IMessenger messenger;

        public FileBrowserViewModel(
            TrackHypervisor trackHypervisor,
            StatusService statusService,
            IMessenger messenger)
        {
            this.trackHypervisor = trackHypervisor;
            this.statusService = statusService;
            this.messenger = messenger;
            directoryViewModel.PropertyChanged += DirectoryViewModel_PropertyChanged;
        }

        private void DirectoryViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DirectoryViewModel.SelectedDirectory))
            {
                LoadTracksAsync(directoryViewModel.SelectedDirectory ?? directoryViewModel.FlattenedTree.FirstOrDefault());
            }
        }

        [ObservableProperty]
        private DirectoryTreeViewModel directoryViewModel = new DirectoryTreeViewModel();
        
        public ObservableCollection<TrackViewModel> Tracks { get; } = new();

        [ObservableProperty]
        private TrackViewModel selectedTrack;

        partial void OnSelectedTrackChanged(TrackViewModel value)
        {
            if (value == null)
            {
                return;
            }

            messenger.Send(new TrackSelectedMessage(value.Metadata));
        }

        private async Task LoadTracksAsync(DirectoryNode? directory)
        {
            // TODO cache these directory loads.
            if (directory == null)
            {
                return;
            }

            statusService.SetStatus(Major: $"Loading Track metadata in {directory.Name}...");

            Tracks.Clear();
            var channel = Channel.CreateUnbounded<string>(); // filenames
            var output = Channel.CreateUnbounded<TrackMetadata>(); // processed tracks
            int maxConcurrency = 8;

            var files = Directory.EnumerateFiles(directory.FullPath, "*", SearchOption.AllDirectories);

            // Producer: enqueue file paths
            var producer = Task.Run(async () =>
            {
                foreach (var file in files)
                {
                    if (!supportedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    await channel.Writer.WriteAsync(file).ConfigureAwait(false);
                }
                channel.Writer.Complete();
            });

            // Worker pool
            var workers = Enumerable.Range(0, maxConcurrency).Select(_ => Task.Run(async () =>
            {
                Task Completion = channel.Reader.Completion;

                while (Completion.Status != TaskStatus.RanToCompletion ||
                        Completion.Status != TaskStatus.Faulted ||
                        Completion.Status != TaskStatus.Canceled) 
                {
                    if (!await channel.Reader.WaitToReadAsync().ConfigureAwait(false))
                    {
                        return;
                    }


                    var file = await channel.Reader.ReadAsync().ConfigureAwait(false);

                    try
                    {
                        var track = await trackHypervisor.LoadTrackMetadataAsync(file);
                        await output.Writer.WriteAsync(track);
                        statusService.SetStatus($"Loaded MetaData for {track.Name}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error loading {file}: {ex.Message}");
                    }
                }
            })).ToList();

            // Once all workers finish, signal end of output
            var monitor = Task.WhenAll(workers).ContinueWith(_ => output.Writer.Complete());

            while (true)
            {
                if (!await output.Reader.WaitToReadAsync().ConfigureAwait(false))
                {
                    break;
                }

                var metadata = await output.Reader.ReadAsync().ConfigureAwait(false);

                TrackViewModel trackViewModel = new()
                {
                    Metadata = metadata
                };

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Tracks.Add(trackViewModel);
                });
            }

            await Task.WhenAll(producer, monitor);

            statusService.SetStatus(
                Minor: $"Loaded Metadata for {files.Count()} tracks",
                Major: $"Loading Metadata for tracks in {directory.Name} DONE!");
        }
    }
}

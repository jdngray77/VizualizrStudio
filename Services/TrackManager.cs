using CommunityToolkit.Mvvm.Messaging;
using Services.Messages;
using TagLib;
using ViewModels;
using File = TagLib.File;

namespace Services
{
    public class TrackManager
    {
        private Dictionary<string, TrackMetadata> TrackDatabase = new();
        private readonly IMessenger messenger;

        public TrackManager(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public void SelectTrack(TrackMetadata track)
        {
            // TODO ideally this service should know about the players.
            messenger.Send(new TrackSelectedMessage(track));
        }

        public Task<TrackMetadata> LoadTrackMetadataAsync(string filepath)
        {
            lock (TrackDatabase)
            {
                if (TrackDatabase.TryGetValue(filepath, out var metadata))
                {
                    return Task.FromResult(metadata);
                }
            }

            return Task.Run(() =>
            {
                File file;
                try
                {
                    file = TagLib.File.Create(filepath);
                } catch(Exception e)
                {
                    return new TrackMetadata()
                    {
                        Name = Path.GetFileName(filepath)
                    };
                }

                TrackMetadata trackMetadata = new TrackMetadata()
                {
                    Name = file.Tag.Title,
                    Artist = String.Join(", ", file.Tag.Performers),
                    Album = file.Tag.Album,
                    Year = file.Tag.Year,
                    FilePath = filepath
                };
            
                var pictures = file.Tag.Pictures;
                if (pictures.Length > 0)
                {
                    trackMetadata.AlbumArt = pictures.First().Data.Data;
                }

                lock (TrackDatabase)
                {
                    TrackDatabase[filepath] = trackMetadata;
                }
                
                return trackMetadata;
            });
        }
    }
}

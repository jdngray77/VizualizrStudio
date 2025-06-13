using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using NAudio.Wave;
using Vizualizr.Backend.Audio.Player;
using Vizualizr.Backend.Messaging;
using File = TagLib.File;

namespace Vizualizr.Backend.Audio
{
    /// <summary>
    /// Overseer for all tracks.
    ///
    /// Loads and caches tracks from disk.
    /// </summary>
    // TODO EFFECTIVE MEMORY LEAK; track samples for songs no-longer in play are kept in memory.
    public class TrackHypervisor
    {
        private Dictionary<string, Track> TrackDatabase = new();
        private Dictionary<string, TrackMetadata> MetaDatabase = new();

        private readonly IMessenger messenger;
        private readonly DeckManager deckManager;
        private readonly StatusService statusService;
        
        public TrackHypervisor(
            IMessenger messenger, 
            DeckManager deckManager,
            StatusService statusService)
        {
            this.messenger = messenger;
            this.deckManager = deckManager;
            this.statusService = statusService;
        }

        /// <summary>
        /// Selects the track for play, causing it to be loaded onto a chosen deck
        /// in the following order:
        ///
        /// - First player with no track loaded
        /// - If all players have a track loaded, the first player which is not playing.
        /// - If all players are playing, the first player which is not master.
        ///
        /// Load occurs in this order:
        /// - The deck is first provided metadata
        /// - track samples are loaded from disk, or from memory.
        /// - The deck is provided with the track
        /// - The track is analyzed for beat info.
        /// - A global message of <see cref="TrackAnalysisFinished"/> is broadcast.
        /// 
        /// </summary>
        /// <param name="meta"></param>
        public async Task<(Track?, Deck?)?> LoadToDeckAsync(TrackMetadata meta)
        {
            Deck? deck = ChooseDeckForLoad();

            if (deck == null)
            {
                Console.WriteLine("No available track decks to load the track.");
                statusService.SetStatus(Major: "No decks available to load track.", highlightMajor: true);
                return null;
            }
            
            // load metadata
            deck.LoadTrackLite(meta);

            // Background load the track data
            Track? track = await LoadTrackAsync(meta).ConfigureAwait(false);
            
            if (track == null)
            {
                // Load failed.
                deck.Unload();
                statusService.SetStatus(Major: $"Failed to load {meta.Name}", highlightMajor: true);
                return (null, deck);
            }

            // Should be redundant, but belt and braces.
            track.Metadata ??= meta;
            
            // Load audio data into the deck; it can now start to be played.
            deck.LoadTrack(track);
            
            // Analyze the audio
            try
            {
                BeatInfo info = await AnalyzeBpmAsync(track).ConfigureAwait(false);
                track.BeatInfo = info;
                messenger.Send(new TrackAnalysisFinished(track));
            }
            catch (Exception)
            {
                statusService.SetStatus(
                    Major: $"Track Analysis Failed for {track.Metadata.Name}!",
                    highlightMajor: true);
            }

            return (track, deck);
        }

        /// <summary>
        /// Selects a deck to load a track to.
        ///
        /// If all decks are playing, the deck returned will be one that is already loaded
        /// and playing.
        /// 
        /// </summary>
        /// <returns>
        /// - Null if there are no decks
        ///
        /// - The most suitible deck to load a track to.
        /// </returns>
        private Deck? ChooseDeckForLoad()
        {
            var decks = deckManager.GetDecks();

            if (decks.Count == 0)
            {
                return null;
            }

            // Best candidate: First player with no track loaded
            var targetDeck = decks.FirstOrDefault(p => !p.TrackLoaded);

            // Second best: First player that is not playing
            if (targetDeck == null)
            {
                targetDeck = decks.FirstOrDefault(p => !p.IsPlaying);
            }

            // Worst case - First deck that is not master. Will be actively playing.
            // if (targetPlayer == null)
            //     targetPlayer = trackPlayers.FirstOrDefault(p => !p.IsMaster);

            return targetDeck;
        }

        /// <summary>
        /// Returns a known track BPM, or analyzes the track to estimate the BPM.
        /// <param name="track">Song metadata</param>
        /// <param name="samples">Audio samples for the song.</param>
        /// </summary>
        /// <param name="forceCalculate">When true, will ignore metadata on disk and will always calculate it.</param>
        /// <param name="saveOnDisk">If true, when BPM is calculated, will attempt to write back to disk in the track's metadata.</param>
        /// <returns>The known or estimated BPM of the track.</returns>
        /// <exception cref="InvalidOperationException">if input data is bad.</exception>
        public async Task<BeatInfo> AnalyzeBpmAsync(
            Track track,
            bool forceCalculate = false,
            bool saveOnDisk = true)
        {
            statusService.SetStatus(Minor: $"Analyzing {track.Metadata.Name}");
            
            if (track?.Samples == null ||
                track.Samples.Length == 0 ||
                track.SampleRate == 0 ||
                track.Channels == 0)
            {
                throw new InvalidOperationException("Input data not sufficient.");
            }
            
            if ((track!.Metadata?.BPM != null && track.Metadata.BPM != 0) && !forceCalculate)
            {
                // Already know the bpm, only need to determine the first beat position.
                int firstBeat = AudioUtilities.DetectFirstBeat(track.SampleRate , track.Metadata.BPM, track.Samples);
                BeatInfo info = new BeatInfo()
                {
                    Bpm = track.Metadata.BPM,
                    IndexOfFirstBeat = firstBeat
                };

                return info;
            }
            
            // Don't know any beat info, or have been told to ignore known data.
            BeatInfo beatInfo = await AudioUtilities.AnalyzeBeatAsync(
                track!.Samples,
                track.Channels,
                track.SampleRate)
                .ConfigureAwait(false);

            if (saveOnDisk)
            {
                try
                {
                    File file = File.Create(track.Metadata.FilePath);
                    if (file.Writeable)
                    {
                        file.Tag.BeatsPerMinute = Convert.ToUInt32(beatInfo.Bpm);
                        file.Save();
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("Failed to save analyzed bpm to song metadata.");
                }
            }
            
            return beatInfo;
        }

        public async Task<Track?> LoadTrackAsync(TrackMetadata metadata)
        {
            lock (TrackDatabase)
            {
                if (TrackDatabase.TryGetValue(metadata.FilePath, out var t))
                {
                    return t;
                }
            }
            
            if (!System.IO.File.Exists(metadata.FilePath))
            {
                return null;
            }

            
            (float[] samples, int channels, int sampleRate, TimeSpan length) audio =
                await LoadAudioAsync(metadata.FilePath).ConfigureAwait(false);
            
            Track track = new Track()
            {
                Metadata = metadata,
                Samples = audio.samples,
                SampleRate = audio.sampleRate,
                Channels = audio.channels,
                Length = audio.length,
            };
            
            lock (TrackDatabase)
            {
                TrackDatabase.Add(metadata.FilePath, track);
            }

            return track;
        }

        private async Task<(
            float[] samples,
            int channels,
            int sampleRate,
            TimeSpan length
            )> LoadAudioAsync(string path)
        {
            // TODO remove dependency on NAudio.
            await using var audioReader = new AudioFileReader(path);
            var sampleReader = audioReader.ToStereo();
            var buffer = new float[(audioReader.Length * 2) / sizeof(float)];
            int read = sampleReader.Read(buffer, 0, buffer.Length);
            return (
                buffer.Take(read).ToArray(),
                audioReader.WaveFormat.Channels,
                audioReader.WaveFormat.SampleRate,
                audioReader.TotalTime
            );
        }

        public Task<TrackMetadata> LoadTrackMetadataAsync(string filepath)
        {
            lock (MetaDatabase)
            {
                if (MetaDatabase.TryGetValue(filepath, out var t))
                {
                    return Task.FromResult(t);
                }
            }

            return Task.Run(() =>
            {
                File file;
                try
                {
                    file = File.Create(filepath);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to load track metadata.");
                    return new TrackMetadata()
                    {
                        Name = Path.GetFileName(filepath)
                    };
                }

                TrackMetadata trackMetadata = new TrackMetadata()
                {
                    Name = file.Tag.Title,
                    Artist = string.Join(", ", file.Tag.Performers),
                    Album = file.Tag.Album,
                    Year = file.Tag.Year,
                    FilePath = filepath,
                    BPM = file.Tag.BeatsPerMinute
                };

                var pictures = file.Tag.Pictures;
                if (pictures.Length > 0)
                {
                    trackMetadata.AlbumArt = pictures.First().Data.Data;
                }

                lock (MetaDatabase)
                {
                    MetaDatabase.Add(filepath, trackMetadata);
                }

                return trackMetadata;
            });
        }
    }
}

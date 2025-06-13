namespace Vizualizr.Backend.Audio
{
    public class TrackMetadata
    {
        public string Name { get; set; }
        public byte[] AlbumArt { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string FilePath { get; set; }
        public uint Year { get; set; }
        
        public string Key { get; set; }
        
        /// <summary>
        /// The bpm loaded from disk, from the file's metadata.
        ///
        /// Don't use this. Use BPM from the track's beat info.
        /// </summary>
        public float BPM { get; set; } = 0;
    }
}

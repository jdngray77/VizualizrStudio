namespace ViewModels
{
    public class TrackMetadata
    {
        public string Name { get; set; }
        public byte[] AlbumArt { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Length { get; set; }
        public string FilePath { get; set; }
        public uint Year { get; set; }
        
        // Analysis
        public string Key { get; set; }
        public string BPM { get; set; }
    }
}

namespace Vizualizr.MidiMapper.Utility
{
    public static class FileType
    {
        public static readonly FilePickerFileType MidiMap = new(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".xml", ".mmap" } },
            { DevicePlatform.macOS, new[] { "xml", ".mmap" } },
        });
    }
}

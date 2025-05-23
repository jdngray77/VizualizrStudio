namespace Visualizer.Controls
{
    public interface IWaveform
    {
        public float[]? Samples { get; set; }
    
        public float Zoom { get; set; }
    
        public float Progress { get; set; }
    }
}
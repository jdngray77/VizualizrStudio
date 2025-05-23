using System.Diagnostics.CodeAnalysis;

namespace Services.Registry
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum RegistryKey
    {
        I_Waveform_SamplesPerPixel,
        I_Waveform_RenderType,              // 0 = DrawableWaveform, 1 = SLGLWaveform
    }
}
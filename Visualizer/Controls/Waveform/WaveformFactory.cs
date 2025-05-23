using Services.Registry;
using Visualizer.Controls.Waveform;

namespace Visualizer.Controls
{
    public class WaveformFactory
    {
        private readonly IRegistry registry;

        public WaveformFactory() : this(Registry.Shared)
        {
        }

        public WaveformFactory(IRegistry registry)
        {
            this.registry = registry;
        }

        public (IWaveform waveform, IView sceneObject) CreateWaveform()
        {
            int type = registry.ReadInt(RegistryKey.I_Waveform_RenderType);

            switch (type)
            {
                case 0:
                    DrawableWaveform waveformD = new DrawableWaveform();
                    GraphicsView gv = new GraphicsView();
                    gv.MinimumHeightRequest = 400;
                    gv.MinimumWidthRequest = 400;
                    gv.Drawable = waveformD;
                    return (waveformD, gv);
                
                default:
                case 1:
                    SKGLWaveform waveformGL = new SKGLWaveform();
                    waveformGL.MinimumHeightRequest = 400;
                    waveformGL.MinimumWidthRequest = 400;
                    return (waveformGL, waveformGL);
                    break;
            }
        }   
    }
}
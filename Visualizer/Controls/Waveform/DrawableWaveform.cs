using System.Diagnostics;
using System.Runtime;
using Services.Registry;
using Registry = Services.Registry.Registry;
using RegistryKey = Services.Registry.RegistryKey;

namespace Visualizer.Controls.Waveform
{
    /**
     * A waveform rendered with MAUI's IDrawable.
     *
     * May be slower than SKGLWaveform, which takes more focus on GPU acceleration.
     */
    public class DrawableWaveform : BindableObject, IWaveform, IDrawable
    {
        #region BindableProperties

        public static readonly BindableProperty SamplesProperty = BindableProperty.Create(
            propertyName: nameof(Samples),
            returnType: typeof(float[]),
            declaringType: typeof(DrawableWaveform),
            defaultValue: null,
            propertyChanged: OnSamplesChanged);



        public static readonly BindableProperty ZoomProperty = BindableProperty.Create(
            nameof(Zoom), typeof(float), typeof(SKGLWaveform), 1f,
            coerceValue: (b, v) => Math.Max(1f, (float)v!));
        
        public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
            nameof(Progress), typeof(float), typeof(SKGLWaveform), 0f,
            coerceValue: (b, v) => Math.Clamp((float)v!, 0f, 1f));

        #endregion
        
        #region Properties
        
        /**
         * Track progress
         */
        public float Progress
        {
            get => (float)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        /**
         * Zoom level.
         */
        public float Zoom
        {
            get => (float)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        /**
         * The audio samples to render.
         */
        public float[] Samples
        {
            get => (float[])GetValue(SamplesProperty);
            set => SetValue(SamplesProperty, value);
        }

        private static void OnSamplesChanged(BindableObject bindable, object oldValue, object newValue)
        {
        }
        #endregion

        #region Fields
        private readonly int samplesPerPixel;
        
        #endregion

        public DrawableWaveform() : this(Registry.Shared) { }

        public DrawableWaveform(IRegistry registry)
        {
            samplesPerPixel = registry.ReadInt(RegistryKey.I_Waveform_SamplesPerPixel);
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            //canvas.SaveState();
            DrawWaveform(canvas, dirtyRect);
            //canvas.RestoreState();
        }

        private void DrawWaveform(ICanvas canvas, RectF rect)
        {
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(rect);

            canvas.StrokeColor = Colors.Red;
            canvas.DrawLine(rect.Width * .5f, 0, rect.Width * .5f, rect.Height);

            if (Samples == null || Samples.Length == 0)
            {
                return;
            }

            int samplesPerPixelBase = 256;
            int samplesPerPixel = (int)(samplesPerPixelBase / Zoom);
            samplesPerPixel = Math.Max(1, samplesPerPixel);

            int pixelsToDraw = (int)rect.Width;
            int totalVisibleSamples = pixelsToDraw * samplesPerPixel;
            int trackSamples = Samples.Length;
            int startSample = (int)(Progress * trackSamples) - totalVisibleSamples / 2;
            startSample = Math.Clamp(startSample, 0, Math.Max(0, trackSamples - totalVisibleSamples));

            float midY = rect.Center.Y;
            canvas.StrokeSize = 1;
            for (int px = 0; px < pixelsToDraw; px++)
            {
                int sampleIdx = startSample + px * samplesPerPixel;
                if (sampleIdx >= trackSamples) break;
                int endIdx = Math.Min(sampleIdx + samplesPerPixel, trackSamples);
                
                float[] slice = Samples[sampleIdx..endIdx];
                float max = slice.Max(Math.Abs);

                // colour gradient
                float hue = 200f * (1 - max);
                canvas.StrokeColor = ColorFromHSV(hue, 0.9f, 1f);

                float lineHeight = max * rect.Height / 2;
                float x = rect.X + px;
                canvas.DrawLine(x, midY - lineHeight, x, midY + lineHeight);
            }
        }

        private static Color ColorFromHSV(float hue, float sat, float val)
        {
            int hi = (int)Math.Floor(hue / 60) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);
            float v = val;
            float p = val * (1 - sat);
            float q = val * (1 - f * sat);
            float t = val * (1 - (1 - f) * sat);

            float r = 0, g = 0, b = 0;
            switch (hi)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }
            return new Color(r, g, b);
        }
    }
}
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Vizualizr.Controls.Waveform
{
    /// <summary>
    /// GPU‑accelerated waveform view for .NET MAUI using SKGLView.
    /// The red playhead stays centered; waveform scrolls beneath it and aligns accurately.
    /// </summary>
    public class SKGLWaveform : SKGLView, IWaveform
    {
        #region BindableProperties
        public static readonly BindableProperty SamplesProperty = BindableProperty.Create(
            nameof(Samples), typeof(float[]), typeof(SKGLWaveform),
            propertyChanged: (a, b, c) => ((SKGLWaveform)a).SamplesChanged(a,b,c));

        public static readonly BindableProperty ZoomProperty = BindableProperty.Create(
            nameof(Zoom), typeof(float), typeof(SKGLWaveform), 1f,
            coerceValue: (b, v) => Math.Max(1f, (float)v!),
            propertyChanged: (a, b, c) => ((SKGLWaveform)a).ZoomChanged(a,b,c));

        #endregion

        #region PropertyChanged
        private void SamplesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            PrecomputeBuckets();
        }
        
        private void ZoomChanged(BindableObject b, object o, object n)
        {
            PrecomputeBuckets();
        }
        #endregion
        
        #region Properties
        
        public float Progress
        {
            get;
            set;
        } = 0f;

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
        public float[]? Samples
        {
            get => (float[]?)GetValue(SamplesProperty);
            set => SetValue(SamplesProperty, value);
        }
        #endregion

        #region Fields
        private readonly int samplesPerPixel;
        
        #endregion

        
        public SKGLWaveform()
        {
            HasRenderLoop = true;  // Enables continuous GPU redraws
            PaintSurface += OnPaintSurface;
        }

        private float[]? _cachedMin;
        private float[]? _cachedMax;
        private int _cachedBucketCount;
        private float _cachedZoom = -1f;
        private float[]? _cachedSamples;
        
        private readonly SKPaint paint = new SKPaint
        {
            IsAntialias = false,
            Color = new SKColor(0x33, 0xA8, 0xFF, 0xFF),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10,
        };
            
        private readonly SKPaint playheadPaint = new SKPaint
        {
            Color = SKColors.Red,
            StrokeWidth = 2,
            IsAntialias = false,
            Style = SKPaintStyle.Stroke
        };

        private void PrecomputeBuckets()
        {
            var samples = Samples;
            if (samples == null || samples.Length == 0)
            {
                _cachedMin = null;
                _cachedMax = null;
                _cachedBucketCount = 0;
                return;
            }

            if (_cachedZoom == Zoom && _cachedSamples == samples)
                return; // no change, skip recompute

            int bucketCount = (int)(samples.Length / Zoom);
            bucketCount = Math.Max(1, bucketCount);

            _cachedMin = new float[bucketCount];
            _cachedMax = new float[bucketCount];

            int samplesPerBucket = (int)Zoom;
            for (int b = 0; b < bucketCount; b++)
            {
                int start = b * samplesPerBucket;
                int end = Math.Min(start + samplesPerBucket, samples.Length);

                float min = samples[start];
                float max = samples[start];

                for (int i = start + 1; i < end; i++)
                {
                    float s = samples[i];
                    if (s < min) min = s;
                    if (s > max) max = s;
                }

                _cachedMin[b] = min;
                _cachedMax[b] = max;
            }

            _cachedBucketCount = bucketCount;
            _cachedZoom = Zoom;
            _cachedSamples = samples;
        }

        private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
        {
            if (_cachedMin == null || _cachedMax == null || _cachedBucketCount == 0)
                return;
            
            BatchBegin();
            
            var canvas = e.Surface.Canvas;

            int width = e.Info.Width;
            int height = e.Info.Height;

            int visibleSampleCount = _cachedBucketCount;

            float waveformPixelLength = visibleSampleCount;
            float centerX = width / 2f;
            float maxOffset = waveformPixelLength - width;
            if (maxOffset < 0) maxOffset = 0;

            float offsetX = centerX - (maxOffset * Progress);
            float midY = height / 2f;

            using var path = new SKPath();

            for (int x = 0; x < width; x++)
            {
                float waveformX = x - offsetX;
                int bucketIndex = (int)waveformX;
                if (bucketIndex < 0 || bucketIndex >= visibleSampleCount)
                    continue;

                float min = _cachedMin[bucketIndex];
                float max = _cachedMax[bucketIndex];

                float y1 = midY - max * midY;
                float y2 = midY - min * midY;

                path.MoveTo(x, y1);
                path.LineTo(x, y2);
            }

            canvas.DrawPath(path, paint);

            canvas.DrawLine(centerX, 0, centerX, height, playheadPaint);
            
            BatchCommit();
        }
    }
}

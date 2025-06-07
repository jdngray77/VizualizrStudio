namespace Vizualizr;


public class DjDrawable : IDrawable
{
    private const float KnobRadius = 60f;
    private const float KnobValue = 0.75f; // 0..1
    private const bool SwitchOn = true;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Enable GPU acceleration when possible (handled by MAUI/Skia back‑end automatically)

        canvas.SaveState();
        DrawKnob(canvas, new PointF(150, 150), KnobRadius, KnobValue);
        DrawToggle(canvas, new RectF(300, 120, 120, 60), SwitchOn);
        canvas.RestoreState();
    }

    private static void DrawKnob(ICanvas canvas, PointF center, float radius, float value)
    {
        // Outer ring
        canvas.StrokeColor = Colors.DarkGray;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(center, radius);

        // Indicator arc (–135° to 135°)
        var startDeg = -135f;
        var sweepDeg = 270f * value;
        canvas.FillColor = Colors.LightSkyBlue;
        canvas.DrawArc(center.X - radius, center.Y - radius, radius * 2, radius * 2, startDeg, sweepDeg, true, true);

        // Knob center
        // Direction line instead of solid center
        var angleRad = (startDeg + sweepDeg) * (float)Math.PI / 180f; // current value angle
        var innerR = radius * 0.2f;  // start a bit away from exact center
        var outerR = radius * 0.6f;  // length of pointer
        var start = new PointF(center.X + innerR * (float)Math.Cos(angleRad),
            center.Y + innerR * (float)Math.Sin(angleRad));
        var end = new PointF(center.X + outerR * (float)Math.Cos(angleRad),
            center.Y + outerR * (float)Math.Sin(angleRad));

        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 6;
        canvas.DrawLine(start, end);
    }

    private static void DrawToggle(ICanvas canvas, RectF rect, bool isOn)
    {
        var radius = rect.Height / 2;
        // Track
        canvas.FillColor = isOn ? Colors.PaleGreen : Colors.LightGray;
        canvas.StrokeColor = Colors.DarkGray;
        canvas.StrokeSize = 2;
        canvas.FillRoundedRectangle(rect, radius);
        canvas.DrawRoundedRectangle(rect, radius);

        // Thumb
        var thumbCenterX = isOn ? rect.Right - radius : rect.Left + radius;
        canvas.FillColor = Colors.White;
        canvas.FillCircle(new PointF(thumbCenterX, rect.Center.Y), radius * 0.9f);
        canvas.DrawCircle(new PointF(thumbCenterX, rect.Center.Y), radius * 0.9f);
    }
}

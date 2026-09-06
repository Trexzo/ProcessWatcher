namespace ProcessWatcher;

internal sealed class SparklineControl : Control
{
    private readonly Queue<double> _values = new();
    private const int MaxPoints = 52;

    public double Maximum { get; set; } = 100;

    public SparklineControl()
    {
        DoubleBuffered = true;
        BackColor = Theme.Surface;
        Size = new Size(180, 44);
    }

    public void AddValue(double value)
    {
        _values.Enqueue(value);
        while (_values.Count > MaxPoints)
            _values.Dequeue();

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var gridPen = new Pen(Color.FromArgb(45, Theme.Border), 1);
        for (int i = 1; i < 4; i++)
        {
            int y = Height * i / 4;
            g.DrawLine(gridPen, 0, y, Width, y);
        }

        var values = _values.ToArray();
        if (values.Length < 2)
            return;

        var points = new PointF[values.Length];
        float step = Width / (float)Math.Max(values.Length - 1, 1);

        for (int i = 0; i < values.Length; i++)
        {
            double normalized = Maximum <= 0 ? 0 : Math.Clamp(values[i] / Maximum, 0, 1);
            float x = i * step;
            float y = (float)(Height - 3 - normalized * (Height - 6));
            points[i] = new PointF(x, y);
        }

        using var glowPen = new Pen(Color.FromArgb(45, Theme.Accent), 5);
        using var linePen = new Pen(Theme.Accent, 2);
        g.DrawLines(glowPen, points);
        g.DrawLines(linePen, points);
    }
}

namespace ProcessWatcher;

internal sealed class UsageBar : Control
{
    private double _value;

    public double Value
    {
        get => _value;
        set
        {
            _value = Math.Clamp(value, 0, 100);
            Invalidate();
        }
    }

    public UsageBar()
    {
        DoubleBuffered = true;
        Height = 5;
        BackColor = Theme.Surface3;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        int fill = (int)Math.Round(Width * (_value / 100.0));
        if (fill <= 0) return;

        Color color = _value >= 90 ? Theme.Danger :
                      _value >= 75 ? Theme.Warning :
                      Theme.Accent;

        using var brush = new SolidBrush(color);
        e.Graphics.FillRectangle(brush, 0, 0, fill, Height);
    }
}

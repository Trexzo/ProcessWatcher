using System.Drawing.Drawing2D;

namespace ProcessWatcher;

internal class RoundedPanel : Panel
{
    public int CornerRadius { get; set; } = 10;
    public Color BorderColor { get; set; } = Theme.Border;
    public float BorderWidth { get; set; } = 1F;

    public RoundedPanel()
    {
        DoubleBuffered = true;
        BackColor = Theme.Surface;
        Resize += (_, _) => UpdateRegion();
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0)
            return;

        using var path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), CornerRadius);
        Region?.Dispose();
        Region = new Region(path);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new RectangleF(
            BorderWidth / 2F,
            BorderWidth / 2F,
            Width - BorderWidth,
            Height - BorderWidth);

        using var pen = new Pen(BorderColor, BorderWidth);
        using var path = CreateRoundedPath(Rectangle.Round(rect), CornerRadius);
        e.Graphics.DrawPath(pen, path);
    }

    internal static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int r = Math.Max(1, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
        int d = r * 2;

        path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();

        return path;
    }
}

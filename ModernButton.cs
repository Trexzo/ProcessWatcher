using System.Drawing.Drawing2D;

namespace ProcessWatcher;

internal sealed class ModernButton : Button
{
    private bool _hovered;

    public ModernButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Theme.Surface2;
        ForeColor = Theme.Text;
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI Semibold", 9F);
        Height = 34;

        MouseEnter += (_, _) =>
        {
            _hovered = true;
            Invalidate();
        };

        MouseLeave += (_, _) =>
        {
            _hovered = false;
            Invalidate();
        };

        Resize += (_, _) => UpdateRegion();
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0)
            return;

        using var path = RoundedPanel.CreateRoundedPath(new Rectangle(0, 0, Width, Height), 8);
        Region?.Dispose();
        Region = new Region(path);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedPanel.CreateRoundedPath(rect, 8);
        using var fill = new SolidBrush(_hovered ? Theme.Surface3 : Theme.Surface2);
        using var border = new Pen(_hovered ? Theme.AccentMuted : Theme.Border, 1F);

        pevent.Graphics.FillPath(fill, path);
        pevent.Graphics.DrawPath(border, path);

        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            rect,
            ForeColor,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis);
    }
}

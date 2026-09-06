namespace ProcessWatcher;

internal sealed class ModernScrollBar : Control
{
    private int _maximum;
    private int _largeChange = 1;
    private int _value;
    private bool _dragging;
    private int _dragOffset;

    public event EventHandler? ValueChanged;

    public int Maximum
    {
        get => _maximum;
        set
        {
            _maximum = Math.Max(0, value);
            Value = Math.Min(_value, _maximum);
            Invalidate();
        }
    }

    public int LargeChange
    {
        get => _largeChange;
        set
        {
            _largeChange = Math.Max(1, value);
            Invalidate();
        }
    }

    public int Value
    {
        get => _value;
        set
        {
            int next = Math.Clamp(value, 0, _maximum);
            if (_value == next)
                return;

            _value = next;
            ValueChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public ModernScrollBar()
    {
        Width = 11;
        BackColor = Theme.Surface;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }

    public void ScrollBy(int rows) => Value += rows;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var track = new Rectangle(
            Width / 2 - 1,
            6,
            2,
            Math.Max(1, Height - 12));

        using var trackBrush = new SolidBrush(Theme.Surface3);
        e.Graphics.FillRectangle(trackBrush, track);

        var thumb = GetThumbRectangle();
        if (thumb.Height <= 0)
            return;

        using var thumbBrush = new SolidBrush(Theme.AccentMuted);
        e.Graphics.FillRectangle(thumbBrush, thumb);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        var thumb = GetThumbRectangle();
        if (thumb.Contains(e.Location))
        {
            _dragging = true;
            _dragOffset = e.Y - thumb.Top;
            Capture = true;
            return;
        }

        if (e.Y < thumb.Top)
            ScrollBy(-LargeChange);
        else if (e.Y > thumb.Bottom)
            ScrollBy(LargeChange);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_dragging || _maximum <= 0)
            return;

        int trackTop = 6;
        int trackHeight = Math.Max(1, Height - 12);
        var thumb = GetThumbRectangle();
        int travel = Math.Max(1, trackHeight - thumb.Height);
        int y = Math.Clamp(e.Y - _dragOffset - trackTop, 0, travel);

        Value = (int)Math.Round(y / (double)travel * _maximum);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _dragging = false;
        Capture = false;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        ScrollBy(e.Delta > 0 ? -3 : 3);
    }

    private Rectangle GetThumbRectangle()
    {
        int trackTop = 6;
        int trackHeight = Math.Max(1, Height - 12);

        if (_maximum <= 0)
            return new Rectangle(3, trackTop, Math.Max(3, Width - 6), trackHeight);

        int totalRows = _maximum + _largeChange;
        int thumbHeight = Math.Max(28, (int)(trackHeight * (_largeChange / (double)Math.Max(totalRows, 1))));
        thumbHeight = Math.Min(trackHeight, thumbHeight);

        int travel = Math.Max(0, trackHeight - thumbHeight);
        int top = trackTop + (int)Math.Round(travel * (_value / (double)_maximum));

        return new Rectangle(3, top, Math.Max(3, Width - 6), thumbHeight);
    }
}

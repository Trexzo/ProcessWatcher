using System.Drawing.Drawing2D;

namespace ProcessWatcher;

internal sealed class ModernDropDown : Control
{
    private readonly List<string> _items = new();
    private int _selectedIndex = -1;
    private bool _hovered;

    public event EventHandler? SelectedIndexChanged;

    public IReadOnlyList<string> Items => _items;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_items.Count == 0)
            {
                _selectedIndex = -1;
                return;
            }

            int next = Math.Clamp(value, 0, _items.Count - 1);
            if (_selectedIndex == next)
                return;

            _selectedIndex = next;
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public string SelectedText =>
        _selectedIndex >= 0 && _selectedIndex < _items.Count
            ? _items[_selectedIndex]
            : string.Empty;

    public ModernDropDown(IEnumerable<string> items)
    {
        _items.AddRange(items);
        _selectedIndex = _items.Count > 0 ? 0 : -1;

        Height = 32;
        BackColor = Theme.Surface2;
        ForeColor = Theme.Text;
        Font = new Font("Segoe UI", 9F);
        Cursor = Cursors.Hand;
        DoubleBuffered = true;

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

        MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                ShowMenu();
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

    private void ShowMenu()
    {
        var menu = new ContextMenuStrip
        {
            ShowImageMargin = false,
            ShowCheckMargin = false,
            BackColor = Theme.Surface2,
            ForeColor = Theme.Text,
            Renderer = new DarkMenuRenderer(),
            Padding = new Padding(4)
        };

        for (int i = 0; i < _items.Count; i++)
        {
            int index = i;
            var item = new ToolStripMenuItem(_items[i])
            {
                AutoSize = false,
                Width = Math.Max(Width - 8, 140),
                Height = 30,
                ForeColor = Theme.Text,
                BackColor = Theme.Surface2,
                Font = new Font("Segoe UI", 9F)
            };

            if (i == _selectedIndex)
                item.Text = "•  " + item.Text;

            item.Click += (_, _) => SelectedIndex = index;
            menu.Items.Add(item);
        }

        menu.Closed += (_, _) => menu.Dispose();
        menu.Show(this, new Point(0, Height + 2));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        using var path = RoundedPanel.CreateRoundedPath(rect, 8);
        using var fill = new SolidBrush(_hovered ? Theme.Surface3 : Theme.Surface2);
        using var border = new Pen(_hovered ? Theme.AccentMuted : Theme.Border, 1F);

        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);

        var textRect = new Rectangle(11, 0, Math.Max(0, Width - 40), Height);
        TextRenderer.DrawText(
            e.Graphics,
            SelectedText,
            Font,
            textRect,
            ForeColor,
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis);

        int cx = Width - 18;
        int cy = Height / 2;
        using var chevron = new Pen(Theme.Muted, 1.6F)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };

        e.Graphics.DrawLine(chevron, cx - 4, cy - 2, cx, cy + 2);
        e.Graphics.DrawLine(chevron, cx, cy + 2, cx + 4, cy - 2);
    }

    private sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColorTable())
        {
            RoundedEdges = true;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = Theme.Text;
            base.OnRenderItemText(e);
        }
    }

    private sealed class DarkColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Theme.Surface2;
        public override Color ImageMarginGradientBegin => Theme.Surface2;
        public override Color ImageMarginGradientMiddle => Theme.Surface2;
        public override Color ImageMarginGradientEnd => Theme.Surface2;
        public override Color MenuItemSelected => Theme.AccentSoft;
        public override Color MenuItemBorder => Theme.AccentMuted;
        public override Color MenuBorder => Theme.Border;
    }
}

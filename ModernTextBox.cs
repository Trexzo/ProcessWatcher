using System.Drawing.Drawing2D;

namespace ProcessWatcher;

internal sealed class ModernTextBox : UserControl
{
    private readonly TextBox _textBox = new();

    public string PlaceholderText
    {
        get => _textBox.PlaceholderText;
        set => _textBox.PlaceholderText = value;
    }

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string Text
    {
        get => _textBox.Text;
        set => _textBox.Text = value ?? string.Empty;
    }

    public ModernTextBox()
    {
        DoubleBuffered = true;
        BackColor = Theme.Surface2;
        ForeColor = Theme.Text;
        Height = 32;
        Padding = new Padding(12, 6, 12, 5);

        _textBox.BorderStyle = BorderStyle.None;
        _textBox.BackColor = Theme.Surface2;
        _textBox.ForeColor = Theme.Text;
        _textBox.Font = new Font("Segoe UI", 9F);
        _textBox.Dock = DockStyle.Fill;

        _textBox.TextChanged += (_, _) =>
        {
            base.Text = _textBox.Text;
            OnTextChanged(EventArgs.Empty);
        };

        Controls.Add(_textBox);

        Resize += (_, _) => UpdateRegion();
        Enter += (_, _) => _textBox.Focus();
        Click += (_, _) => _textBox.Focus();
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0)
            return;

        using var path = RoundedPanel.CreateRoundedPath(new Rectangle(0, 0, Width, Height), 8);
        Region?.Dispose();
        Region = new Region(path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var path = RoundedPanel.CreateRoundedPath(
            new Rectangle(0, 0, Width - 1, Height - 1), 8);
        using var pen = new Pen(_textBox.Focused ? Theme.AccentMuted : Theme.Border, 1F);
        e.Graphics.DrawPath(pen, path);
    }
}

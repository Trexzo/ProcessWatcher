namespace ProcessWatcher;

internal sealed class MetricCard : RoundedPanel
{
    private readonly Label _title = new();
    private readonly Label _value = new();
    private readonly Label _subtitle = new();
    private readonly SparklineControl _sparkline = new();
    private readonly UsageBar _usage = new();

    public SparklineControl Sparkline => _sparkline;

    public MetricCard(string title)
    {
        CornerRadius = 12;
        BorderColor = Theme.Border;
        BackColor = Theme.Surface;
        Padding = new Padding(16);
        Margin = new Padding(0);

        _title.Text = title.ToUpperInvariant();
        _title.Font = new Font("Segoe UI Semibold", 8.5F);
        _title.ForeColor = Theme.Muted;
        _title.AutoSize = true;
        _title.Location = new Point(16, 14);

        _value.Font = new Font("Segoe UI Semibold", 21F);
        _value.ForeColor = Theme.Text;
        _value.AutoSize = true;
        _value.Location = new Point(14, 35);
        _value.Text = "—";

        _subtitle.Font = new Font("Segoe UI", 8.25F);
        _subtitle.ForeColor = Theme.Muted;
        _subtitle.AutoSize = true;
        _subtitle.MaximumSize = new Size(260, 0);
        _subtitle.Location = new Point(17, 75);

        _usage.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        _usage.Location = new Point(16, 101);
        _usage.Size = new Size(210, 5);

        _sparkline.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        _sparkline.Location = new Point(16, 114);
        _sparkline.Size = new Size(210, 38);

        Controls.AddRange(new Control[] { _title, _value, _subtitle, _usage, _sparkline });

        Resize += (_, _) =>
        {
            int innerWidth = Math.Max(30, Width - 32);
            _usage.Width = innerWidth;
            _sparkline.Width = innerWidth;
            _subtitle.MaximumSize = new Size(innerWidth, 0);
        };
    }

    public void SetValue(string value, string subtitle, double? percent = null)
    {
        _value.Text = value;
        _subtitle.Text = subtitle;
        _usage.Visible = percent.HasValue;

        if (percent.HasValue)
            _usage.Value = percent.Value;
    }
}

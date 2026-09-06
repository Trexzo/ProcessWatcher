using System.Diagnostics;

namespace ProcessWatcher;

public sealed class MainForm : Form
{
    private readonly SystemSampler _systemSampler = new();
    private readonly ProcessSampler _processSampler = new();
    private readonly System.Windows.Forms.Timer _timer = new();

    private readonly MetricCard _cpuCard = new("CPU");
    private readonly MetricCard _memoryCard = new("Memory");
    private readonly MetricCard _diskCard = new("Disk");
    private readonly MetricCard _networkCard = new("Network");

    private readonly DataGridView _processGrid = new();
    private readonly ModernScrollBar _gridScroll = new();
    private readonly ModernTextBox _searchBox = new();
    private readonly ModernDropDown _sortBox = new(new[]
    {
        "CPU usage",
        "Memory usage",
        "Process name",
        "PID"
    });
    private readonly ModernDropDown _refreshBox = new(new[]
    {
        "500 ms",
        "1 second",
        "2 seconds",
        "5 seconds"
    });

    private readonly Label _uptimeLabel = new();
    private readonly Label _processCountLabel = new();
    private readonly Label _statusLabel = new();
    private readonly Label _selectedTitle = new();
    private readonly Label _selectedStats = new();
    private readonly Label _selectedPath = new();

    private IReadOnlyList<ProcessRow> _processes = Array.Empty<ProcessRow>();

    public MainForm()
    {
        Text = "ProcessWatcher";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 720);
        Size = new Size(1260, 820);
        BackColor = Theme.Background;
        ForeColor = Theme.Text;
        Font = new Font("Segoe UI", 9F);

        BuildUi();

        _timer.Interval = 1000;
        _timer.Tick += (_, _) => SampleNow();

        Shown += (_, _) =>
        {
            TryEnableDarkTitleBar();
            SampleNow();
            _timer.Start();
        };
    }

    private void BuildUi()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 78,
            BackColor = Theme.Sidebar
        };

        var accentBar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 4,
            BackColor = Theme.Accent
        };

        var logo = new Label
        {
            Text = "PROCESSWATCHER",
            Font = new Font("Segoe UI Semibold", 19F),
            ForeColor = Theme.Text,
            AutoSize = true,
            Location = new Point(26, 14)
        };

        var subtitle = new Label
        {
            Text = "Real-time Windows process & performance monitor",
            Font = new Font("Segoe UI", 9F),
            ForeColor = Theme.Muted,
            AutoSize = true,
            Location = new Point(28, 46)
        };

        _uptimeLabel.AutoSize = true;
        _uptimeLabel.ForeColor = Theme.Muted;
        _uptimeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _uptimeLabel.Text = "Uptime —";
        _uptimeLabel.Location = new Point(1000, 30);

        header.Controls.Add(accentBar);
        header.Controls.Add(logo);
        header.Controls.Add(subtitle);
        header.Controls.Add(_uptimeLabel);

        header.Resize += (_, _) =>
            _uptimeLabel.Left = header.Width - _uptimeLabel.Width - 24;

        var root = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Background,
            Padding = new Padding(18)
        };

        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 172,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Theme.Background
        };

        for (int i = 0; i < 4; i++)
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        AddMetricCard(cards, _cpuCard, 0);
        AddMetricCard(cards, _memoryCard, 1);
        AddMetricCard(cards, _diskCard, 2);
        AddMetricCard(cards, _networkCard, 3);

        var body = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 10,
            BackColor = Theme.Background
        };

        bool splitConfigured = false;
        body.SizeChanged += (_, _) =>
        {
            if (splitConfigured || body.ClientSize.Width < 900)
                return;

            int available = body.ClientSize.Width - body.SplitterWidth;
            const int leftMin = 600;
            const int rightMin = 240;

            int desired = Math.Clamp(
                (int)(available * 0.72),
                leftMin,
                available - rightMin);

            body.SplitterDistance = desired;
            body.Panel1MinSize = leftMin;
            body.Panel2MinSize = rightMin;
            splitConfigured = true;
        };

        body.Panel1.Controls.Add(BuildProcessPanel());
        body.Panel2.Controls.Add(BuildDetailsPanel());

        root.Controls.Add(body);
        root.Controls.Add(cards);

        var statusBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Theme.Sidebar
        };

        _statusLabel.AutoSize = true;
        _statusLabel.ForeColor = Theme.Muted;
        _statusLabel.Location = new Point(14, 7);
        _statusLabel.Text = "Starting…";
        statusBar.Controls.Add(_statusLabel);

        Controls.Add(root);
        Controls.Add(statusBar);
        Controls.Add(header);
    }

    private static void AddMetricCard(TableLayoutPanel cards, MetricCard card, int column)
    {
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(column == 0 ? 0 : 6, 0, column == 3 ? 0 : 6, 12);
        cards.Controls.Add(card, column, 0);
    }

    private Control BuildProcessPanel()
    {
        var panel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Surface,
            CornerRadius = 12,
            BorderColor = Theme.Border
        };

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 76,
            BackColor = Theme.Surface
        };

        var title = new Label
        {
            Text = "Processes",
            Font = new Font("Segoe UI Semibold", 14F),
            ForeColor = Theme.Text,
            AutoSize = true,
            Location = new Point(18, 13)
        };

        _processCountLabel.Text = "—";
        _processCountLabel.ForeColor = Theme.Muted;
        _processCountLabel.AutoSize = true;
        _processCountLabel.Location = new Point(20, 43);

        _searchBox.PlaceholderText = "Search process, PID or window title…";
        _searchBox.Size = new Size(265, 32);
        _searchBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _searchBox.TextChanged += (_, _) => RenderProcesses();

        _sortBox.Size = new Size(132, 32);
        _sortBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _sortBox.SelectedIndexChanged += (_, _) => RenderProcesses();

        header.Controls.Add(title);
        header.Controls.Add(_processCountLabel);
        header.Controls.Add(_searchBox);
        header.Controls.Add(_sortBox);

        header.Resize += (_, _) =>
        {
            _sortBox.Left = header.Width - _sortBox.Width - 15;
            _searchBox.Left = _sortBox.Left - _searchBox.Width - 10;
            _searchBox.Top = _sortBox.Top = 21;
        };

        ConfigureProcessGrid();

        var gridHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Surface,
            Padding = new Padding(0, 0, 5, 0)
        };

        _gridScroll.Dock = DockStyle.Right;
        _gridScroll.ValueChanged += (_, _) => ApplyCustomScroll();

        _processGrid.Dock = DockStyle.Fill;
        _processGrid.MouseWheel += (_, e) =>
        {
            _gridScroll.ScrollBy(e.Delta > 0 ? -3 : 3);
            if (e is HandledMouseEventArgs handled)
                handled.Handled = true;
        };
        _processGrid.Resize += (_, _) => UpdateCustomScrollMetrics();

        gridHost.Controls.Add(_processGrid);
        gridHost.Controls.Add(_gridScroll);

        panel.Controls.Add(gridHost);
        panel.Controls.Add(header);
        return panel;
    }

    private Control BuildDetailsPanel()
    {
        var panel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Surface,
            CornerRadius = 12,
            BorderColor = Theme.Border,
            Padding = new Padding(18)
        };

        var title = new Label
        {
            Text = "Selected process",
            Font = new Font("Segoe UI Semibold", 12F),
            ForeColor = Theme.Text,
            AutoSize = true,
            Location = new Point(18, 16)
        };

        _selectedTitle.Text = "Nothing selected";
        _selectedTitle.Font = new Font("Segoe UI Semibold", 15F);
        _selectedTitle.ForeColor = Theme.Text;
        _selectedTitle.AutoEllipsis = true;
        _selectedTitle.Location = new Point(18, 58);
        _selectedTitle.Size = new Size(245, 28);

        _selectedStats.Text = "Select a row to inspect it.";
        _selectedStats.ForeColor = Theme.Muted;
        _selectedStats.Location = new Point(18, 100);
        _selectedStats.Size = new Size(245, 90);

        _selectedPath.Text = "";
        _selectedPath.ForeColor = Theme.Dim;
        _selectedPath.Location = new Point(18, 195);
        _selectedPath.Size = new Size(245, 74);
        _selectedPath.AutoEllipsis = true;

        var openButton = new ModernButton
        {
            Text = "Open file location",
            Location = new Point(18, 286),
            Size = new Size(190, 34)
        };
        openButton.Click += (_, _) => OpenSelectedLocation();

        var copyButton = new ModernButton
        {
            Text = "Copy PID",
            Location = new Point(18, 328),
            Size = new Size(190, 34)
        };
        copyButton.Click += (_, _) => CopySelectedPid();

        var divider = new Panel
        {
            BackColor = Theme.Border,
            Height = 1,
            Location = new Point(18, 389),
            Size = new Size(245, 1),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };

        var settingsTitle = new Label
        {
            Text = "Refresh interval",
            ForeColor = Theme.Muted,
            AutoSize = true,
            Location = new Point(18, 409)
        };

        _refreshBox.SelectedIndex = 1;
        _refreshBox.Location = new Point(18, 436);
        _refreshBox.Size = new Size(160, 32);
        _refreshBox.SelectedIndexChanged += (_, _) =>
        {
            _timer.Interval = _refreshBox.SelectedIndex switch
            {
                0 => 500,
                1 => 1000,
                2 => 2000,
                _ => 5000
            };
        };

        panel.Controls.AddRange(new Control[]
        {
            title, _selectedTitle, _selectedStats, _selectedPath,
            openButton, copyButton, divider, settingsTitle, _refreshBox
        });

        panel.Resize += (_, _) =>
        {
            int width = Math.Max(160, panel.ClientSize.Width - 36);
            _selectedTitle.Width = width;
            _selectedStats.Width = width;
            _selectedPath.Width = width;
            divider.Width = width;
            openButton.Width = Math.Min(190, width);
            copyButton.Width = Math.Min(190, width);
        };

        return panel;
    }

    private void ConfigureProcessGrid()
    {
        _processGrid.BackgroundColor = Theme.Surface;
        _processGrid.BorderStyle = BorderStyle.None;
        _processGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _processGrid.GridColor = Theme.Border;
        _processGrid.EnableHeadersVisualStyles = false;
        _processGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        _processGrid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Surface2;
        _processGrid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Muted;
        _processGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Surface2;
        _processGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.Muted;
        _processGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
        _processGrid.ColumnHeadersHeight = 36;

        _processGrid.DefaultCellStyle.BackColor = Theme.Surface;
        _processGrid.DefaultCellStyle.ForeColor = Theme.Text;
        _processGrid.DefaultCellStyle.SelectionBackColor = Theme.AccentSoft;
        _processGrid.DefaultCellStyle.SelectionForeColor = Theme.Text;
        _processGrid.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);

        _processGrid.RowHeadersVisible = false;
        _processGrid.AllowUserToAddRows = false;
        _processGrid.AllowUserToDeleteRows = false;
        _processGrid.AllowUserToResizeRows = false;
        _processGrid.ReadOnly = true;
        _processGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _processGrid.MultiSelect = false;
        _processGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _processGrid.RowTemplate.Height = 31;
        _processGrid.ScrollBars = ScrollBars.None;

        _processGrid.Columns.Add("Name", "Process");
        _processGrid.Columns.Add("Pid", "PID");
        _processGrid.Columns.Add("Cpu", "CPU");
        _processGrid.Columns.Add("Memory", "Memory");
        _processGrid.Columns.Add("Threads", "Threads");

        _processGrid.Columns["Name"]!.FillWeight = 180;
        _processGrid.Columns["Pid"]!.FillWeight = 55;
        _processGrid.Columns["Cpu"]!.FillWeight = 60;
        _processGrid.Columns["Memory"]!.FillWeight = 78;
        _processGrid.Columns["Threads"]!.FillWeight = 58;

        _processGrid.SelectionChanged += (_, _) => ShowSelectedDetails();

        var menu = new ContextMenuStrip
        {
            ShowImageMargin = false,
            BackColor = Theme.Surface2,
            ForeColor = Theme.Text
        };
        menu.Items.Add("Open file location", null, (_, _) => OpenSelectedLocation());
        menu.Items.Add("Copy PID", null, (_, _) => CopySelectedPid());
        _processGrid.ContextMenuStrip = menu;
    }

    private void SampleNow()
    {
        try
        {
            var snapshot = _systemSampler.Sample();

            _cpuCard.SetValue(
                $"{snapshot.CpuPercent:0}%",
                "total processor usage",
                snapshot.CpuPercent);
            _cpuCard.Sparkline.AddValue(snapshot.CpuPercent);

            _memoryCard.SetValue(
                $"{snapshot.Memory.Percent:0}%",
                $"{FormatBinaryBytes(snapshot.Memory.UsedBytes)} / {FormatBinaryBytes(snapshot.Memory.TotalBytes)}",
                snapshot.Memory.Percent);
            _memoryCard.Sparkline.AddValue(snapshot.Memory.Percent);

            _diskCard.SetValue(
                $"{snapshot.Disk.Percent:0}%",
                $"{snapshot.Disk.Drive}  {FormatDecimalBytes((ulong)snapshot.Disk.UsedBytes)} / {FormatDecimalBytes((ulong)snapshot.Disk.TotalBytes)} • {FormatDecimalBytes((ulong)snapshot.Disk.FreeBytes)} free",
                snapshot.Disk.Percent);
            _diskCard.Sparkline.AddValue(snapshot.Disk.Percent);

            double totalNetwork = snapshot.Network.BytesReceivedPerSecond +
                                  snapshot.Network.BytesSentPerSecond;

            _networkCard.SetValue(
                FormatRate(totalNetwork),
                $"↓ {FormatRate(snapshot.Network.BytesReceivedPerSecond)}   ↑ {FormatRate(snapshot.Network.BytesSentPerSecond)}");
            _networkCard.Sparkline.Maximum = Math.Max(1024 * 1024, _networkCard.Sparkline.Maximum * 0.94);
            _networkCard.Sparkline.Maximum = Math.Max(_networkCard.Sparkline.Maximum, totalNetwork * 1.15);
            _networkCard.Sparkline.AddValue(totalNetwork);

            _uptimeLabel.Text = $"Uptime {FormatUptime(snapshot.Uptime)}";

            int? selectedPid = GetSelectedPid();

            _processes = _processSampler.Sample();
            RenderProcesses(selectedPid);

            _statusLabel.Text = $"Updated {DateTime.Now:T}  •  {_processes.Count:N0} processes";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Sampling error: {ex.Message}";
        }
    }

    private void RenderProcesses(int? restorePid = null)
    {
        string query = _searchBox.Text.Trim();

        IEnumerable<ProcessRow> rows = _processes;

        if (!string.IsNullOrWhiteSpace(query))
        {
            rows = rows.Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Pid.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(p.WindowTitle) &&
                 p.WindowTitle.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        rows = _sortBox.SelectedIndex switch
        {
            1 => rows.OrderByDescending(p => p.MemoryBytes),
            2 => rows.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            3 => rows.OrderBy(p => p.Pid),
            _ => rows.OrderByDescending(p => p.CpuPercent).ThenByDescending(p => p.MemoryBytes)
        };

        var visible = rows.Take(300).ToArray();

        _processGrid.SuspendLayout();
        _processGrid.Rows.Clear();

        int rowToSelect = -1;

        foreach (var p in visible)
        {
            int index = _processGrid.Rows.Add(
                p.Name,
                p.Pid,
                $"{p.CpuPercent:0.0}%",
                FormatBinaryBytes((ulong)Math.Max(0, p.MemoryBytes)),
                p.Threads);

            _processGrid.Rows[index].Tag = p;

            if (restorePid == p.Pid)
                rowToSelect = index;

            if (p.CpuPercent >= 25)
                _processGrid.Rows[index].Cells["Cpu"].Style.ForeColor = Theme.Warning;

            if (p.MemoryBytes >= 2L * 1024 * 1024 * 1024)
                _processGrid.Rows[index].Cells["Memory"].Style.ForeColor = Theme.Warning;
        }

        _processGrid.ResumeLayout();

        _processCountLabel.Text =
            string.IsNullOrWhiteSpace(query)
                ? $"{_processes.Count:N0} running"
                : $"{visible.Length:N0} matching";

        if (rowToSelect >= 0 && rowToSelect < _processGrid.Rows.Count)
        {
            _processGrid.ClearSelection();
            _processGrid.Rows[rowToSelect].Selected = true;
            _processGrid.CurrentCell = _processGrid.Rows[rowToSelect].Cells[0];
        }
        else if (_processGrid.Rows.Count > 0 && _processGrid.SelectedRows.Count == 0)
        {
            _processGrid.Rows[0].Selected = true;
        }

        UpdateCustomScrollMetrics();
        ShowSelectedDetails();
    }

    private void UpdateCustomScrollMetrics()
    {
        if (_processGrid.Rows.Count == 0)
        {
            _gridScroll.Maximum = 0;
            _gridScroll.LargeChange = 1;
            _gridScroll.Value = 0;
            return;
        }

        int visibleRows = Math.Max(1, _processGrid.DisplayedRowCount(false));
        _gridScroll.LargeChange = visibleRows;
        _gridScroll.Maximum = Math.Max(0, _processGrid.Rows.Count - visibleRows);

        try
        {
            int first = _processGrid.FirstDisplayedScrollingRowIndex;
            if (first >= 0)
                _gridScroll.Value = Math.Min(first, _gridScroll.Maximum);
        }
        catch
        {
            _gridScroll.Value = 0;
        }
    }

    private void ApplyCustomScroll()
    {
        if (_processGrid.Rows.Count == 0)
            return;

        int index = Math.Clamp(_gridScroll.Value, 0, _processGrid.Rows.Count - 1);

        try
        {
            _processGrid.FirstDisplayedScrollingRowIndex = index;
        }
        catch
        {
            // Grid may be between layout passes.
        }
    }

    private int? GetSelectedPid()
    {
        if (_processGrid.CurrentRow?.Tag is ProcessRow row)
            return row.Pid;

        if (_processGrid.CurrentRow is not null &&
            int.TryParse(_processGrid.CurrentRow.Cells["Pid"].Value?.ToString(), out int pid))
            return pid;

        return null;
    }

    private void ShowSelectedDetails()
    {
        if (_processGrid.CurrentRow?.Tag is not ProcessRow row)
        {
            _selectedTitle.Text = "Nothing selected";
            _selectedStats.Text = "Select a row to inspect it.";
            _selectedPath.Text = "";
            return;
        }

        _selectedTitle.Text = row.Name;
        _selectedStats.Text =
            $"PID       {row.Pid}\r\n" +
            $"CPU       {row.CpuPercent:0.0}%\r\n" +
            $"Memory    {FormatBinaryBytes((ulong)Math.Max(0, row.MemoryBytes))}\r\n" +
            $"Threads   {row.Threads}";

        try
        {
            using var process = Process.GetProcessById(row.Pid);
            _selectedPath.Text = process.MainModule?.FileName ?? row.WindowTitle;
        }
        catch
        {
            _selectedPath.Text = string.IsNullOrWhiteSpace(row.WindowTitle)
                ? "Path unavailable"
                : row.WindowTitle;
        }
    }

    private void OpenSelectedLocation()
    {
        int? pid = GetSelectedPid();
        if (pid is null)
            return;

        try
        {
            using var process = Process.GetProcessById(pid.Value);
            string? path = process.MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException();

            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"")
            {
                UseShellExecute = true
            });
        }
        catch
        {
            MessageBox.Show(
                this,
                "Unable to access this process's executable.",
                "ProcessWatcher",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private void CopySelectedPid()
    {
        int? pid = GetSelectedPid();
        if (pid is not null)
            Clipboard.SetText(pid.Value.ToString());
    }

    private void TryEnableDarkTitleBar()
    {
        try
        {
            int enabled = 1;
            NativeMethods.DwmSetWindowAttribute(Handle, 20, ref enabled, sizeof(int));
        }
        catch
        {
            // Best effort only.
        }
    }

    private static string FormatBinaryBytes(ulong bytes)
    {
        string[] units = { "B", "KiB", "MiB", "GiB", "TiB" };
        double value = bytes;
        int unit = 0;

        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.#} {units[unit]}";
    }

    private static string FormatDecimalBytes(ulong bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double value = bytes;
        int unit = 0;

        while (value >= 1000 && unit < units.Length - 1)
        {
            value /= 1000;
            unit++;
        }

        return $"{value:0.##} {units[unit]}";
    }

    private static string FormatRate(double bytesPerSecond)
    {
        if (bytesPerSecond < 1024) return $"{bytesPerSecond:0} B/s";
        if (bytesPerSecond < 1024 * 1024) return $"{bytesPerSecond / 1024:0.#} KiB/s";
        if (bytesPerSecond < 1024 * 1024 * 1024) return $"{bytesPerSecond / (1024 * 1024):0.#} MiB/s";
        return $"{bytesPerSecond / (1024 * 1024 * 1024):0.##} GiB/s";
    }

    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";

        return $"{uptime.Hours}h {uptime.Minutes}m";
    }
}

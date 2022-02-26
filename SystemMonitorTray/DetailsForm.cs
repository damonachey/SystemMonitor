using Networking;

using System.Windows.Forms.DataVisualization.Charting;

namespace SystemMonitorTray;

public partial class DetailsForm : Form
{
    private INetworkMonitor networkMonitor = default!;
    private Chart chart = default!;
    private List<(string Description, Label Value, Range Range)> totals = default!;
    private Range selectedRange = Range.Hour;
    private Unit selectedUnit = Unit.MB;

    public DetailsForm(INetworkMonitor networkMonitor)
    {
        BackColor = Properties.Settings.Default.applicationBackgroundColor;
        ForeColor = Properties.Settings.Default.applicationForegroundColor;
        MinimumSize = new(600, 300);
        StartPosition = FormStartPosition.Manual;

        selectedRange = (Range)Properties.Settings.Default.detailsSelectedRange;
        selectedUnit = (Unit)Properties.Settings.Default.detailsSelectedUnit;

        InitializeComponent();
        InitializeChartSettingsButtons();
        InitializeChart();
        InitializeTotals();
        InitializeNetworkMonitor(networkMonitor);

        Load += OnLoad;
    }

    private void SaveWindowPosition()
    {
        if (Size.Width >= MinimumSize.Width && Size.Height >= MinimumSize.Height)
        {
            Properties.Settings.Default.detailsFormLocation = Location;
            Properties.Settings.Default.detailsFormSize = Size;
        }
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        Location = Properties.Settings.Default.detailsFormLocation;
        Size = Properties.Settings.Default.detailsFormSize;

        if (!FormUtilities.IsOnScreen(Location, Size))
        {
            SetDesktopLocation(0, 0);
        }

        FormClosing += (s, e) => SaveWindowPosition();
        LocationChanged += (s, e) => SaveWindowPosition();
        Shown += (s, e) => UpdateNetworkData();
        SizeChanged += (s, e) => SaveWindowPosition();
    }

    private void InitializeChartSettingsButtons()
    {
        var ranges = new RadioButtonGroup();
        ranges.CreateRadioButton("1h", Range.Hour);
        ranges.CreateRadioButton("1d", Range.Day);
        ranges.CreateRadioButton("24h", Range.Hours24);
        ranges.CreateRadioButton("1w", Range.Week);
        ranges.CreateRadioButton("7d", Range.Days7);
        ranges.CreateRadioButton("1M", Range.Month);
        ranges.CreateRadioButton("30d", Range.Days30);
        ranges.CreateRadioButton("All", Range.All);

        for (var i = 0; i < ranges.Count; i++)
        {
            var range = ranges[i];
            range.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            range.Font = new(Font, FontStyle.Bold);
            range.Location = new Point(i * 40, 0);
            range.Size = new(40, 25);

            if ((Range)range.Tag == selectedRange)
            {
                range.PerformClick();
            }

            range.Click += (s, e) =>
            {
                selectedRange = (Range)range.Tag;
                Properties.Settings.Default.detailsSelectedRange = (int)selectedRange;
                UpdateChart();
            };
        }

        Controls.AddRange(ranges);

        var units = new RadioButtonGroup();
        units.CreateRadioButton(nameof(Unit.B), Unit.B);
        units.CreateRadioButton(nameof(Unit.KB), Unit.KB);
        units.CreateRadioButton(nameof(Unit.MB), Unit.MB);
        units.CreateRadioButton(nameof(Unit.GB), Unit.GB);
        units.CreateRadioButton(nameof(Unit.TB), Unit.TB);
        units.CreateRadioButton(nameof(Unit.PB), Unit.PB);

        for (var i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            unit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            unit.Font = new(Font, FontStyle.Bold);
            unit.Location = new Point(ClientSize.Width - (units.Count - i) * 40);
            unit.Size = new(40, 25);

            if ((Unit)unit.Tag == selectedUnit)
            {
                unit.PerformClick();
            }

            unit.Click += (s, e) =>
            {
                selectedUnit = (Unit)unit.Tag;
                Properties.Settings.Default.detailsSelectedUnit = (long)selectedUnit;
                UpdateChart();
            };
        }

        Controls.AddRange(units);
    }

    private void InitializeChart()
    {
        chart = new()
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
            BackColor = BackColor,
            ForeColor = ForeColor,
            Location = new Point(0, 25),
            Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 100),
            TabStop = false,
        };

        var legend = new Legend
        {
            Docking = Docking.Bottom,
            BackColor = BackColor,
            ForeColor = ForeColor,
            LegendItemOrder = LegendItemOrder.ReversedSeriesOrder,
        };
        chart.Legends.Add(legend);

        var ca = new ChartArea { BackColor = BackColor };
        ca.AxisX.LabelStyle.ForeColor = ForeColor;
        ca.AxisX.LineColor = ForeColor;
        ca.AxisX.MajorGrid.LineColor = ForeColor;
        ca.AxisX.MajorTickMark.Enabled = false;
        ca.AxisX.MajorTickMark.LineColor = ForeColor;
        ca.AxisY.LabelStyle.ForeColor = ForeColor;
        ca.AxisY.LineColor = ForeColor;
        ca.AxisY.MajorGrid.LineColor = ForeColor;
        ca.AxisY.MajorTickMark.Enabled = false;
        ca.AxisY.MajorTickMark.LineColor = ForeColor;
        ca.AxisY.TitleForeColor = ForeColor;
        chart.ChartAreas.Add(ca);

        var series = new[] {
            new Series("Sent") { Color = Properties.Settings.Default.detailsFormSentChartColor },
            new Series("Received") { Color = Properties.Settings.Default.detailsFormReceivedChartColor },
        };

        foreach (var s in series)
        {
            s.ChartType = SeriesChartType.SplineArea;
            s.CustomProperties = "DrawSideBySide=False";
            s.IsVisibleInLegend = true;
            s.LabelForeColor = ForeColor;
            s.XValueType = ChartValueType.DateTime;
            chart.Series.Add(s);
        };

        Controls.Add(chart);
    }

    private void InitializeTotals()
    {
        totals = new()
        {
            //("Total Hour:", new(), Range.Hour),
            //("Total All:", new(),  Range.All),
            ("Total Day:", new(), Range.Day),
            ("Total 24 Hours:", new(), Range.Hours24),
            ("Total Week:", new(), Range.Week),
            ("Total 7 Days:", new(), Range.Days7),
            ("Total Month:", new(), Range.Month),
            ("Total 30 Days:", new(), Range.Days30),
        };

        for (var i = 0; i < totals.Count; i++)
        {
            var description = new Label { Text = totals[i].Description };
            description.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            description.Location = new Point(i / 2 * 200 + 10, ClientSize.Height - (totals.Count / 3 - i % 2) * description.Height - 10);
            description.Width = 80;
            Controls.Add(description);

            var value = totals[i].Value;

            value.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            value.Location = new Point(description.Location.X + 80, description.Location.Y);
            value.Text = $"- GB";
            value.TextAlign = ContentAlignment.TopRight;
            value.Width = 80;
            Controls.Add(value);
        }
    }

    private void InitializeNetworkMonitor(INetworkMonitor networkMonitor)
    {
        this.networkMonitor = networkMonitor;
        this.networkMonitor.OnUpdate += UpdateNetworkData;
    }

    private void UpdateNetworkData()
    {
        lock (this)
            Invoke(() =>
            {
                UpdateTotals();
                UpdateChart();
            });
    }

    private void UpdateTotals()
    {
        foreach (var total in totals)
        {
            var bytes = GetLogs(total.Range).Sum(log => log.BytesTotal);

            total.Value.Text = $"{bytes / (double)Unit.GB:0.000} {nameof(Unit.GB)}";
        }
    }

    private void UpdateChart()
    {
        chart.Series[0].Points.Clear();
        chart.Series[1].Points.Clear();

        var size = GetLogParameters(selectedRange).Size;
        var divisor = size.Ticks switch
        {
            >= TimeSpan.TicksPerDay * 7 => "week",
            >= TimeSpan.TicksPerDay => "day",
            > TimeSpan.TicksPerHour => $"{size.Hours} hours",
            TimeSpan.TicksPerHour => $"hour",
            > TimeSpan.TicksPerMinute => $"{size.Minutes} minutes",
            _ => $"minute",
        };

        var title = $"{Enum.GetName<Unit>(selectedUnit)} / {divisor}";

        chart.ChartAreas[0].AxisY.Title = title;

        if (selectedRange < Range.Week) chart.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm";
        else chart.ChartAreas[0].AxisX.LabelStyle.Format = "";

        foreach (var log in GetLogs(selectedRange))
        {
            chart.Series["Received"].Points.AddXY(log.Time, log.BytesReceived / (double)selectedUnit);
            chart.Series["Received"].Points.Last().ToolTip = $"{log.Time:M/d/y HH:mm}\n{log.BytesReceived / (double)selectedUnit:0.00} {title}";

            chart.Series["Sent"].Points.AddXY(log.Time, log.BytesTotal / (double)selectedUnit);
            chart.Series["Sent"].Points.Last().ToolTip = $"{log.Time:M/d/y HH:mm}\n{log.BytesSent / (double)selectedUnit:0.00} {title}";
        }
    }

    private IEnumerable<Log> GetLogs(Range range)
    {
        var (earliest, size) = GetLogParameters(range);

        var logs = networkMonitor.Logs
            .Where(log => log.Time >= earliest)
            .GroupBy(log => log.Time.Ticks / size.Ticks)
            .Select(g => new Log
            {
                Time = g.First().Time,
                BytesReceived = g.Sum(log => log.BytesReceived),
                BytesSent = g.Sum(log => log.BytesSent)
            });

        return logs;
    }

    private static (DateTime Earlist, TimeSpan Size) GetLogParameters(Range range)
    {
        return range switch
        {
            Range.Hour => (DateTime.Now.AddHours(-1), TimeSpan.FromMinutes(1)),
            Range.Day => (DateTime.Today, TimeSpan.FromMinutes(60)),
            Range.Hours24 => (DateTime.Now.AddHours(-24), TimeSpan.FromMinutes(60)),
            Range.Week => (DateTime.Now.StartOfWeek(), TimeSpan.FromHours(1)),
            Range.Days7 => (DateTime.Now.AddDays(-7), TimeSpan.FromHours(1)),
            Range.Month => (DateTime.Now.StartOfMonth(), TimeSpan.FromHours(24)),
            Range.Days30 => (DateTime.Now.AddDays(-30), TimeSpan.FromHours(24)),
            Range.All => (DateTime.MinValue, TimeSpan.FromDays(1)),
            _ => throw new ArgumentOutOfRangeException($"Range: {range} not supported"),
        };
    }
}

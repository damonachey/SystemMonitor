using Networking;

using System.Windows.Forms.DataVisualization.Charting;

namespace SystemMonitorTray;

public partial class DetailsForm : Form
{
    private INetworkMonitor networkMonitor = default!;
    private Chart chart = default!;
    private List<Label> totals = default!;
    private Range selectedRange = Range.Hour;
    private Unit selectedUnit = Unit.MB;

    public DetailsForm(INetworkMonitor networkMonitor)
    {
        BackColor = Properties.Settings.Default.applicationBackgroundColor;
        ForeColor = Properties.Settings.Default.applicationForegroundColor;
        MinimumSize = new(600, 300);
        StartPosition = FormStartPosition.Manual;

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
        var ranges = new[]
        {
            ControlCreator.CreateRadioButton(1, "1h", Range.Hour, new(40, 25), new(Font, FontStyle.Bold)),
            ControlCreator.CreateRadioButton(1, "1d", Range.Day),
            ControlCreator.CreateRadioButton(1, "24h", Range.Hours24),
            ControlCreator.CreateRadioButton(1, "1w", Range.Week),
            ControlCreator.CreateRadioButton(1, "7d", Range.Days7),
            ControlCreator.CreateRadioButton(1, "1M", Range.Month),
            ControlCreator.CreateRadioButton(1, "30d", Range.Days30),
            ControlCreator.CreateRadioButton(1, "All", Range.All),
        };

        var units = new[]
        {
            ControlCreator.CreateRadioButton(2, nameof(Unit.B), Unit.B, new(40, 25), new(Font, FontStyle.Bold)),
            ControlCreator.CreateRadioButton(2, nameof(Unit.KB), Unit.KB),
            ControlCreator.CreateRadioButton(2, nameof(Unit.MB), Unit.MB),
            ControlCreator.CreateRadioButton(2, nameof(Unit.GB), Unit.GB),
            ControlCreator.CreateRadioButton(2, nameof(Unit.TB), Unit.TB),
            ControlCreator.CreateRadioButton(2, nameof(Unit.PB), Unit.PB),
        };

        for (var i = 0; i < ranges.Length; i++)
        {
            var range = ranges[i];
            range.Location = new Point(i * range.Width, 0);
            range.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            
            if ((Range)range.Tag == selectedRange)
            {
                range.PerformClick();
            }

            range.Click += (s, e) =>
            {
                selectedRange = (Range)range.Tag;
                UpdateChart();
            };
        }

        for (var i = 0; i < units.Length; i++)
        {
            var unit = units[i];
            unit.Location = new Point(ClientSize.Width - (units.Length - i) * unit.Width);
            unit.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            if ((Unit)unit.Tag == selectedUnit)
            {
                unit.PerformClick();
            }

            unit.Click += (s, e) =>
            {
                selectedUnit = (Unit)unit.Tag;
                UpdateChart();
            };
        }

        Controls.AddRange(ranges);
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
        chart.ChartAreas.Add(ca);

        var seriesSent = new Series
        {
            ChartType = SeriesChartType.SplineArea,
            Color = Properties.Settings.Default.detailsFormSentChartColor,
            CustomProperties = "DrawSideBySide=False",
            IsVisibleInLegend = true,
            LabelForeColor = ForeColor,
            Name = "Sent",
            XValueType = ChartValueType.DateTime,
        };
        chart.Series.Add(seriesSent);

        var seriesReceived = new Series
        {
            ChartType = SeriesChartType.SplineArea,
            Color = Properties.Settings.Default.detailsFormReceivedChartColor,
            CustomProperties = "DrawSideBySide=False",
            IsVisibleInLegend = true,
            LabelForeColor = ForeColor,
            Name = "Received",
            XValueType = ChartValueType.DateTime,
        };
        chart.Series.Add(seriesReceived);

        Controls.Add(chart);
    }

    private void InitializeTotals()
    {
        totals = new()
        {
            //new() { Text = "Total Hour:", Tag = new Label { Tag = Range.Hour } },
            //new() { Text = "Total All:", Tag = new Label { Tag = Range.All } },
            new() { Text = "Total Day:", Tag = new Label { Tag = Range.Day } },
            new() { Text = "Total 24 Hours:", Tag = new Label { Tag = Range.Hours24 } },
            new() { Text = "Total Week:", Tag = new Label { Tag = Range.Week } },
            new() { Text = "Total 7 Days:", Tag = new Label { Tag = Range.Days7 } },
            new() { Text = "Total Month:", Tag = new Label { Tag = Range.Month } },
            new() { Text = "Total 30 Days:", Tag = new Label { Tag = Range.Days30 } },
        };

        for (var i = 0; i < totals.Count; i++)
        {
            totals[i].Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            totals[i].Location = new Point(i / 2 * 200 + 10, ClientSize.Height - (totals.Count / 3 - i % 2) * totals[i].Height - 10);
            totals[i].Width = 80;
            Controls.Add(totals[i]);

            var value = (Label)totals[i].Tag;

            value.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            value.Location = new Point(totals[i].Location.X + 80, totals[i].Location.Y);
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
            var value = (Label)total.Tag;
            var range = (Range)value.Tag;

            var bytes = GetLogs(range).Sum(log => log.BytesTotal);

            value.Text = $"{bytes / (double)Unit.GB:0.000} {nameof(Unit.GB)}";
        }
    }

    private void UpdateChart()
    {
        chart.Series[0].Points.Clear();
        chart.Series[1].Points.Clear();

        if (selectedRange < Range.Week) chart.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm";
        else chart.ChartAreas[0].AxisX.LabelStyle.Format = "";

        foreach (var log in GetLogs(selectedRange))
        {
            chart.Series["Received"].Points.AddXY(log.Time, log.BytesReceived / (double)selectedUnit);
            chart.Series["Sent"].Points.AddXY(log.Time, log.BytesTotal / (double)selectedUnit);
        }
    }

    private IEnumerable<Log> GetLogs(Range range)
    {
        var (earlist, size) = range switch
        {
            Range.Hour => (DateTime.Now.AddHours(-1), TimeSpan.FromMinutes(1)),
            Range.Day => (DateTime.Today, TimeSpan.FromMinutes(15)),
            Range.Hours24 => (DateTime.Now.AddHours(-24), TimeSpan.FromMinutes(15)),
            Range.Week => (DateTime.Now.StartOfWeek(), TimeSpan.FromHours(1)),
            Range.Days7 => (DateTime.Now.AddDays(-7), TimeSpan.FromHours(1)),
            Range.Month => (DateTime.Now.StartOfMonth(), TimeSpan.FromHours(8)),
            Range.Days30 => (DateTime.Now.AddDays(-30), TimeSpan.FromHours(8)),
            Range.All => (DateTime.MinValue, TimeSpan.FromDays(7)),
            _ => throw new ArgumentOutOfRangeException($"Range: {range} not supported"),
        };

        var logs = networkMonitor.Logs
            .Where(log => log.Time >= earlist)
            .GroupBy(log => log.Time.Ticks / size.Ticks)
            .Select(g => new Log
            {
                Time = g.First().Time,
                BytesReceived = g.Sum(log => log.BytesReceived),
                BytesSent = g.Sum(log => log.BytesSent)
            });

        return logs;
    }
}

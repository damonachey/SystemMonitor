using Networking;

using System.Windows.Forms.DataVisualization.Charting;

namespace SystemMonitorTray;

public partial class Details : Form
{
    private static readonly Color backColor = Color.FromArgb(30, 30, 30);
    private static readonly Color foreColor = Color.White;
    private static readonly Padding padding = new(6);

    private INetworkMonitor networkMonitor = default!;
    private Chart chart = default!;
    private ComboBox units = default!;
    private ComboBox range = default!;
    private readonly Label totalHour = new() { AutoSize = true, Padding = padding };
    private readonly Label totalDay = new() { AutoSize = true, Padding = padding };
    private readonly Label total24Hours = new() { AutoSize = true, Padding = padding };
    private readonly Label totalWeek = new() { AutoSize = true, Padding = padding };
    private readonly Label total7Days = new() { AutoSize = true, Padding = padding };
    private readonly Label totalMonth = new() { AutoSize = true, Padding = padding };
    private readonly Label total30Days = new() { AutoSize = true, Padding = padding };

    public Details(INetworkMonitor networkMonitor)
    {
        InitializeComponent();
        InitializeChart();
        InitializeChartConfiguration();
        InitialzeLayout();
        InitializeNetworkMonitor(networkMonitor);

        BackColor = backColor;
        ForeColor = foreColor;
        MinimumSize = new Size(400, 300);
        Resize += (o, e) => chart.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 150);
        Shown += (o, e) => UpdateNetworkData();
    }

    private void InitializeChart()
    {
        chart = new()
        {
            BackColor = backColor,
            ForeColor = foreColor,
            Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 150),
            TabStop = false,
        };
        chart.Titles.Add("Usage").ForeColor = foreColor;

        var legend = new Legend
        {
            Docking = Docking.Bottom,
            BackColor = backColor,
            ForeColor = foreColor,
            LegendItemOrder = LegendItemOrder.ReversedSeriesOrder,
        };
        chart.Legends.Add(legend);

        var ca = new ChartArea { BackColor = backColor };
        ca.AxisX.LabelStyle.ForeColor = foreColor;
        //ca.AxisX.LabelStyle.Format = "HH:mm";
        ca.AxisX.LineColor = foreColor;
        ca.AxisX.MajorGrid.LineColor = foreColor;
        ca.AxisX.MajorTickMark.Enabled = false;
        ca.AxisX.MajorTickMark.LineColor = foreColor;
        ca.AxisY.LabelStyle.ForeColor = foreColor;
        ca.AxisY.LineColor = foreColor;
        ca.AxisY.MajorGrid.LineColor = foreColor;
        ca.AxisY.MajorTickMark.Enabled = false;
        ca.AxisY.MajorTickMark.LineColor = foreColor;
        chart.ChartAreas.Add(ca);

        var seriesSent = new Series
        {
            ChartType = SeriesChartType.SplineArea,
            Color = Color.FromArgb(252, 180, 65),
            IsVisibleInLegend = true,
            LabelForeColor = foreColor,
            Name = "Sent",
            XValueType = ChartValueType.DateTime,
        };
        chart.Series.Add(seriesSent);

        var seriesReceived = new Series
        {
            ChartType = SeriesChartType.SplineArea,
            Color = Color.FromArgb(65, 140, 240),
            IsVisibleInLegend = true,
            LabelForeColor = foreColor,
            Name = "Received",
            XValueType = ChartValueType.DateTime,
        };
        chart.Series.Add(seriesReceived);
    }

    private record class RangeItem(string Name, Range Value);
    private record class UnitItem(string Name, double Value);

    private void InitializeChartConfiguration()
    {
        range = new()
        {
            Anchor = AnchorStyles.Left,
            DisplayMember = "Name",
            Dock = DockStyle.Top,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DropDownWidth = 100,
            ValueMember = "Value",
        };
        range.Items.AddRange(new RangeItem[]
        {
            new("Hour", Range.Hour),
            new("Day", Range.Day),
            new("24 Hours", Range.Hours24),
            new("Week", Range.Week),
            new("7 Days", Range.Days7),
            new("Month", Range.Month),
            new("30 Days", Range.Days30),
            new("All", Range.All),
        });
        range.SelectionChangeCommitted += (o, e) => UpdateNetworkData();
        range.SelectedIndex = 0;

        units = new()
        {
            Anchor = AnchorStyles.Left,
            DisplayMember = "Name",
            Dock = DockStyle.Top,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DropDownWidth = 100,
            ValueMember = "Value",
        };
        units.Items.AddRange(new UnitItem[]
        {
            new(nameof(Constants.Bytes), Constants.Bytes),
            new(nameof(Constants.KB), Constants.KB),
            new(nameof(Constants.MB), Constants.MB),
            new(nameof(Constants.GB), Constants.GB),
            new(nameof(Constants.TB), Constants.TB),
            new(nameof(Constants.PB), Constants.PB),
        });
        units.SelectionChangeCommitted += (o, e) => UpdateNetworkData();
        units.SelectedIndex = 2;
    }

    private void InitialzeLayout()
    {
        var layout = new FlowLayoutPanel { Dock = DockStyle.Fill };
        layout.Controls.Add(chart);

        var minimumSize = new Size(100, 0);

        layout.Controls.Add(new Label { Text = "Range:", AutoSize = true, Padding = padding });
        layout.Controls.Add(range);
        layout.Controls.Add(new Label { Text = "Units:", AutoSize = true, Padding = padding });
        layout.Controls.Add(units);
        layout.SetFlowBreak(units, true);

        layout.Controls.Add(new Label { Text = "Total Hour:", AutoSize = true, MinimumSize = minimumSize, Padding = padding });
        layout.Controls.Add(totalHour);
        layout.SetFlowBreak(totalHour, true);

        layout.Controls.Add(new Label { Text = "Total Day:", AutoSize = true, MinimumSize = minimumSize, Padding = padding });
        layout.Controls.Add(totalDay);
        layout.Controls.Add(new Label { Text = "Total 24 Hours:", AutoSize = true, MinimumSize = minimumSize, Padding = padding });
        layout.Controls.Add(total24Hours);
        layout.SetFlowBreak(total24Hours, true);

        layout.Controls.Add(new Label { Text = "Total Week:", AutoSize = true, MinimumSize = minimumSize, Padding = padding });
        layout.Controls.Add(totalWeek);
        layout.Controls.Add(new Label { Text = "Total 7 Days:", AutoSize = true, MinimumSize = minimumSize, Padding = padding });
        layout.Controls.Add(total7Days);
        layout.SetFlowBreak(total7Days, true);

        layout.Controls.Add(new Label { Text = "Total Month:", AutoSize = true, MinimumSize = minimumSize, Padding = padding });
        layout.Controls.Add(totalMonth);
        layout.Controls.Add(new Label { Text = "Total 30 Days:", AutoSize = true, MinimumSize = minimumSize, Padding = padding });
        layout.Controls.Add(total30Days);
        layout.SetFlowBreak(total30Days, true);

        Controls.Add(layout);
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
        double GetTotal(Range range) => GetLogs(range).Sum(log => log.BytesTotal) / Constants.GB;
        string GetTotalStr(Range range) => $"{GetTotal(range):0.000} {nameof(Constants.GB)}";

        totalHour.Text = GetTotalStr(Range.Hour);
        totalDay.Text = GetTotalStr(Range.Day);
        total24Hours.Text = GetTotalStr(Range.Hours24);
        totalWeek.Text = GetTotalStr(Range.Week);
        total7Days.Text = GetTotalStr(Range.Days7);
        totalMonth.Text = GetTotalStr(Range.Month);
        total30Days.Text = GetTotalStr(Range.Days30);
    }

    private void UpdateChart()
    {
        var unit = (UnitItem)units.SelectedItem;
        var range = ((RangeItem)this.range.SelectedItem).Value;

        chart.Series[0].Points.Clear();
        chart.Series[1].Points.Clear();

        foreach (var log in GetLogs(range))
        {
            chart.Series["Received"].Points.AddXY(log.Time, log.BytesReceived / unit.Value);
            chart.Series["Sent"].Points.AddXY(log.Time, log.BytesTotal / unit.Value);
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
            Range.All => (DateTime.MinValue, TimeSpan.FromDays(1)),
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

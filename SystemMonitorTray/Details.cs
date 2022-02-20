using Networking;

using System.Windows.Forms.DataVisualization.Charting;

namespace SystemMonitorTray;

public partial class Details : Form
{
    private INetworkMonitor networkMonitor = default!;
    private Chart chart = default!;
    private ComboBox units = default!;
    private ComboBox range = default!;
    private Label totalHour = default!;
    private Label totalDay = default!;
    private Label total24Hours = default!;
    private Label totalWeek = default!;
    private Label total7Days = default!;
    private Label totalMonth = default!;
    private Label total30Days = default!;

    private readonly Color backColor = Color.FromArgb(30, 30, 30);
    private readonly Color foreColor = Color.White;

    public Details(INetworkMonitor networkMonitor)
    {
        BackColor = backColor;
        ForeColor = foreColor;

        InitializeComponent();
        InitializeChart();
        InitializeChartConfiguration();
        InitialzeLayout();
        InitializeNetworkMonitor(networkMonitor);

        Shown += (o, e) => UpdateNetworkData();
    }

    private void InitializeChart()
    {
        chart = new()
        {
            Name = "History",
            Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 150),
            TabStop = false,
            BackColor = backColor,
            ForeColor = foreColor,
        };
        chart.Titles.Add("Usage").ForeColor = foreColor;

        Resize += (o, e) => chart.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 150);

        var legend = new Legend
        {
            Docking = Docking.Bottom,
            BackColor = backColor,
            ForeColor = foreColor,
            LegendItemOrder = LegendItemOrder.ReversedSeriesOrder,
        };
        chart.Legends.Add(legend);

        var ca = new ChartArea { BackColor = backColor };
        ca.AxisX.LineColor = foreColor;
        ca.AxisX.LabelStyle.ForeColor = foreColor;
//        ca.AxisX.LabelStyle.Format = "HH:mm";
        ca.AxisX.MajorGrid.LineColor = foreColor;
        ca.AxisX.MajorTickMark.Enabled = false;
        ca.AxisX.MajorTickMark.LineColor = foreColor;
        ca.AxisY.LineColor = foreColor;
        ca.AxisY.LabelStyle.ForeColor = foreColor;
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

        totalHour = new() { AutoSize = true, Padding = new Padding(6) };
        totalDay = new() { AutoSize = true, Padding = new Padding(6) };
        total24Hours = new() { AutoSize = true, Padding = new Padding(6) };
        totalWeek = new() { AutoSize = true, Padding = new Padding(6) };
        total7Days = new() { AutoSize = true, Padding = new Padding(6) };
        totalMonth = new() { AutoSize = true, Padding = new Padding(6) };
        total30Days = new() { AutoSize = true, Padding = new Padding(6) };
    }

    private void InitialzeLayout()
    {
        MinimumSize = new Size(400, 300);

        var layout = new FlowLayoutPanel { Dock = DockStyle.Fill };
        layout.Controls.Add(chart);

        layout.Controls.Add(new Label { Text = "Range:", AutoSize = true, Padding = new Padding(6) });
        layout.Controls.Add(range);
        layout.Controls.Add(new Label { Text = "Units:", AutoSize = true, Padding = new Padding(6) });
        layout.Controls.Add(units);
        layout.SetFlowBreak(units, true);

        layout.Controls.Add(new Label { Text = "Total Hour:", AutoSize = true, MinimumSize = new Size(100, 0), Padding = new Padding(6) });
        layout.Controls.Add(totalHour);
        layout.SetFlowBreak(totalHour, true);

        layout.Controls.Add(new Label { Text = "Total Day:", AutoSize = true, MinimumSize = new Size(100, 0), Padding = new Padding(6) });
        layout.Controls.Add(totalDay);
        layout.Controls.Add(new Label { Text = "Total 24 Hours:", AutoSize = true, MinimumSize = new Size(100, 0), Padding = new Padding(6) });
        layout.Controls.Add(total24Hours);
        layout.SetFlowBreak(total24Hours, true);

        layout.Controls.Add(new Label { Text = "Total Week:", AutoSize = true, MinimumSize = new Size(100, 0), Padding = new Padding(6) });
        layout.Controls.Add(totalWeek);
        layout.Controls.Add(new Label { Text = "Total 7 Days:", AutoSize = true, MinimumSize = new Size(100, 0), Padding = new Padding(6) });
        layout.Controls.Add(total7Days);
        layout.SetFlowBreak(total7Days, true);

        layout.Controls.Add(new Label { Text = "Total Month:", AutoSize = true, MinimumSize = new Size(100, 0), Padding = new Padding(6) });
        layout.Controls.Add(totalMonth);
        layout.Controls.Add(new Label { Text = "Total 30 Days:", AutoSize = true, MinimumSize = new Size(100, 0), Padding = new Padding(6) });
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
                var valueTotalHour = GetLogRange(Range.Hour).Sum(log => log.BytesTotal) / Constants.GB;
                var valueTotalDay = GetLogRange(Range.Day).Sum(log => log.BytesTotal) / Constants.GB;
                var valueTotal24Hour = GetLogRange(Range.Hours24).Sum(log => log.BytesTotal) / Constants.GB;
                var valueTotalWeek = GetLogRange(Range.Week).Sum(log => log.BytesTotal) / Constants.GB;
                var valueTotal7Days = GetLogRange(Range.Days7).Sum(log => log.BytesTotal) / Constants.GB;
                var valueTotalMonth = GetLogRange(Range.Month).Sum(log => log.BytesTotal) / Constants.GB;
                var valueTotal30Days = GetLogRange(Range.Days30).Sum(log => log.BytesTotal) / Constants.GB;

                totalHour.Text = $"{valueTotalHour:0.000} GB";
                totalDay.Text = $"{valueTotalDay:0.000} GB";
                total24Hours.Text = $"{valueTotal24Hour:0.000} GB";
                totalWeek.Text = $"{valueTotalWeek:0.000} GB";
                total7Days.Text = $"{valueTotal7Days:0.000} GB";
                totalMonth.Text = $"{valueTotalMonth:0.000} GB";
                total30Days.Text = $"{valueTotal30Days:0.000} GB";

                chart.Series[0].Points.Clear();
                chart.Series[1].Points.Clear();

                var unit = (UnitItem)units.SelectedItem;
                var range = ((RangeItem)this.range.SelectedItem).Value;

                foreach (var log in GetLogRange(range))
                {
                    chart.Series["Received"].Points.AddXY(log.Time, log.BytesReceived / unit.Value);
                    chart.Series["Sent"].Points.AddXY(log.Time, log.BytesTotal / unit.Value);
                }
            });
    }

    private IEnumerable<Log> GetLogRange(Range range) => range switch
    {
        Range.Hour => networkMonitor.Logs.Where(log => log.Time >= DateTime.Now.AddHours(-1)),
        Range.Day => networkMonitor.Logs.Where(logs => logs.Time >= DateTime.Today)
            .GroupBy(log => log.Time.Ticks / TimeSpan.FromMinutes(15).Ticks)
            .Select(g => new Log
            {
                Time = g.First().Time,
                BytesReceived = g.Sum(log => log.BytesReceived),
                BytesSent = g.Sum(log => log.BytesSent)
            }),
        Range.Hours24 => networkMonitor.Logs.Where(log => log.Time >= DateTime.Now.AddHours(-24))
            .GroupBy(log => log.Time.Ticks / TimeSpan.FromMinutes(15).Ticks)
            .Select(g => new Log
            {
                Time = g.First().Time,
                BytesReceived = g.Sum(log => log.BytesReceived),
                BytesSent = g.Sum(log => log.BytesSent)
            }),
        Range.Week => networkMonitor.Logs.Where(log => log.Time >= DateTime.Now.StartOfWeek())
            .GroupBy(log => log.Time.Ticks / TimeSpan.FromHours(1).Ticks)
            .Select(g => new Log
            {
                Time = g.First().Time,
                BytesReceived = g.Sum(log => log.BytesReceived),
                BytesSent = g.Sum(log => log.BytesSent)
            }),
        Range.Days7 => networkMonitor.Logs.Where(log => log.Time >= DateTime.Now.AddDays(-7))
            .GroupBy(log => log.Time.Ticks / TimeSpan.FromHours(1).Ticks)
            .Select(g => new Log
            {
                Time = g.First().Time,
                BytesReceived = g.Sum(log => log.BytesReceived),
                BytesSent = g.Sum(log => log.BytesSent)
            }),
        Range.Month => networkMonitor.Logs.Where(log => log.Time >= DateTime.Now.StartOfMonth())
            .GroupBy(log => log.Time.Ticks / TimeSpan.FromHours(8).Ticks)
            .Select(g => new Log
            {
                Time = g.First().Time,
                BytesReceived = g.Sum(log => log.BytesReceived),
                BytesSent = g.Sum(log => log.BytesSent)
            }),
        Range.Days30 => networkMonitor.Logs.Where(log => log.Time >= DateTime.Now.AddDays(-30))
            .GroupBy(log => log.Time.Ticks / TimeSpan.FromHours(8).Ticks)
            .Select(g => new Log
            {
                Time = g.First().Time,
                BytesReceived = g.Sum(log => log.BytesReceived),
                BytesSent = g.Sum(log => log.BytesSent)
            }),
        Range.All => networkMonitor.Logs
            .GroupBy(log => log.Time.Ticks / TimeSpan.FromDays(1).Ticks)
            .Select(g => new Log
            {
                Time = g.First().Time,
                BytesReceived = g.Sum(log => log.BytesReceived),
                BytesSent = g.Sum(log => log.BytesSent)
            }),
        _ => throw new ArgumentOutOfRangeException($"Range: {range} not supported"),
    };
}

using Networking;

using System.Windows.Forms.DataVisualization.Charting;

namespace SystemMonitorTray;

public partial class DetailsForm : Form
{
    private static readonly Size minimumSize = new(400, 300);
    private static readonly Padding padding = new(6);
    private static readonly SeriesChartType chartType = SeriesChartType.SplineArea;

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

    public DetailsForm(INetworkMonitor networkMonitor)
    {
        BackColor = Properties.Settings.Default.applicationBackgroundColor;
        ForeColor = Properties.Settings.Default.applicationForegroundColor;
        MinimumSize = minimumSize;
        StartPosition = FormStartPosition.Manual;

        InitializeComponent();
        InitializeChart();
        InitializeChartConfiguration();
        InitialzeLayout();
        InitializeNetworkMonitor(networkMonitor);

        FormClosing += (o, e) => OnFormClosing();
        Load += (o, e) => OnLoad();
        Resize += (o, e) => chart.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 150);
        Shown += (o, e) => UpdateNetworkData();
    }

    private void OnFormClosing()
    {
        Properties.Settings.Default.detailsFormLocation = Location;
        Properties.Settings.Default.detailsFormSize = Size;
    }

    private void OnLoad()
    {
        Location = Properties.Settings.Default.detailsFormLocation;
        Size = Properties.Settings.Default.detailsFormSize;
    }

    private void InitializeChart()
    {
        chart = new()
        {
            BackColor = BackColor,
            ForeColor = ForeColor,
            Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 150),
            //TabStop = false,  // TODO:
        };
        chart.Titles.Add("Usage").ForeColor = ForeColor;

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
            ChartType = chartType,
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
            ChartType = chartType,
            Color = Properties.Settings.Default.detailsFormReceivedChartColor,
            CustomProperties = "DrawSideBySide=False",
            IsVisibleInLegend = true,
            LabelForeColor = ForeColor,
            Name = "Received",
            XValueType = ChartValueType.DateTime,
        };
        chart.Series.Add(seriesReceived);

        // TODO: move to settings page
        chart.KeyDown += (o, e) =>
        {
            if (e.KeyCode == Keys.C)
            {
                chart.Series[0].ChartType = SeriesChartType.Column;
                chart.Series[1].ChartType = SeriesChartType.Column;
            }

            if (e.KeyCode == Keys.S)
            {
                chart.Series[0].ChartType = SeriesChartType.SplineArea;
                chart.Series[1].ChartType = SeriesChartType.SplineArea;
            }
        };
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

        layout.Controls.Add(new Label { Text = "Range:", AutoSize = true, Padding = padding });
        layout.Controls.Add(range);
        layout.Controls.Add(new Label { Text = "Units:", AutoSize = true, Padding = padding });
        layout.Controls.Add(units);
        layout.SetFlowBreak(units, true);

        static Label GetLabel(string text) => new()
        {
            Text = text,
            AutoSize = true,
            MinimumSize = new Size(100, 0),
            Padding = padding
        };

        layout.Controls.Add(GetLabel("Total Hour:"));
        layout.Controls.Add(totalHour);
        layout.SetFlowBreak(totalHour, true);

        layout.Controls.Add(GetLabel("Total Day:"));
        layout.Controls.Add(totalDay);
        layout.Controls.Add(GetLabel("Total 24 Hours:"));
        layout.Controls.Add(total24Hours);
        layout.SetFlowBreak(total24Hours, true);

        layout.Controls.Add(GetLabel("Total Week:"));
        layout.Controls.Add(totalWeek);
        layout.Controls.Add(GetLabel("Total 7 Days:"));
        layout.Controls.Add(total7Days);
        layout.SetFlowBreak(total7Days, true);

        layout.Controls.Add(GetLabel("Total Month:"));
        layout.Controls.Add(totalMonth);
        layout.Controls.Add(GetLabel("Total 30 Days:"));
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

        if (range < Range.Week) chart.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm";
        else chart.ChartAreas[0].AxisX.LabelStyle.Format = "";

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
            Range.Day => (DateTime.Today, TimeSpan.FromMinutes(30)),
            Range.Hours24 => (DateTime.Now.AddHours(-24), TimeSpan.FromMinutes(30)),
            Range.Week => (DateTime.Now.StartOfWeek(), TimeSpan.FromHours(4)),
            Range.Days7 => (DateTime.Now.AddDays(-7), TimeSpan.FromHours(4)),
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
            })
            .Skip(1);

        return logs;
    }
}

using Networking;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;

namespace SystemMonitor;

internal class Settings
{
    public static string FileName { get; private set; } = default!;

    private static Settings instance = default!;

    public static Settings Default => instance
        ?? throw new NullReferenceException("Settings referenced before Load(...)");

    public static void Load(string filename)
    {
        FileName = filename
            ?? throw new ArgumentNullException(nameof(filename));

        if (File.Exists(filename))
        {
            var config = File.ReadAllText(filename);

            instance = JsonSerializer.Deserialize<Settings>(config, GetOptions())
                ?? throw new NullReferenceException(nameof(config));
        }
        else
        {
            instance = new();
            Save();
        }
    }

    public static void Save()
    {
        var config = JsonSerializer.Serialize(instance, GetOptions());

        var directory = Path.GetDirectoryName(FileName)
            ?? throw new NullReferenceException(nameof(FileName));

        Directory.CreateDirectory(directory);
        File.WriteAllText(FileName, config);
    }

    private static JsonSerializerOptions GetOptions()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new ColorConverter());
        options.Converters.Add(new PointConverter());
        options.Converters.Add(new SizeConverter());
        options.Converters.Add(new EnumConverter<SeriesChartType>());
        options.Converters.Add(new EnumConverter<Range>());
        options.Converters.Add(new EnumConverter<Unit>());

        return options;
    }

    public bool ApplicationSound { get; internal set; } = true;
    public bool ApplicationStartWithWindows { get; set; } = false;

    public Color ApplicationBackgroundColor { get; set; } = Color.FromArgb(30, 30, 30);
    public Color ApplicationForegroundColor { get; set; } = Color.White;

    public Point DetailsFormLocation { get; set; } = new(0, 0);
    public Size DetailsFormSize { get; set; } = new(800, 600);
    public Color DetailsFormReceivedChartColor { get; set; } = Color.FromArgb(65, 140, 240);
    public Color DetailsFormSentChartColor { get; set; } = Color.FromArgb(252, 180, 65);
    public SeriesChartType DetailsFormChartType { get; set; } = SeriesChartType.SplineArea;
    public Range DetailsFormSelectedRange { get; set; } = Range.Days7;
    public Unit DetailsFormSelectedUnit { get; set; } = Unit.MB;

    public Point SettingsFormLocation { get; set; } = new(0, 0);
    public Size SettingsFormSize { get; set; } = new(600, 300);
    public long SettingsFormAlertValue { get; set; } = 0;
    public Unit SettingsFormAlertUnit { get; set; } = Unit.GB;
    public Range SettingsFormAlertRange { get; set; } = Range.Day;
}

internal class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()
            ?? throw new ArgumentNullException(nameof(reader));

        var match = Regex.Match(value, @"{\s*A=(?<A>\d+),\s*R=(?<R>\d+),\s*G=(?<G>\d+),\s* B=(?<B>\d+)\s*}");

        return Color.FromArgb(
            int.Parse(match.Groups["A"].Value),
            int.Parse(match.Groups["R"].Value),
            int.Parse(match.Groups["G"].Value),
            int.Parse(match.Groups["B"].Value));
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($@"{{A={value.A}, R={value.R}, G={value.G}, B={value.B}}}");
    }
}

internal class PointConverter : JsonConverter<Point>
{
    public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()
            ?? throw new ArgumentNullException(nameof(reader));

        var match = Regex.Match(value, @"{\s*X=(?<X>\d+),\s*Y=(?<Y>\d+)\s*}");

        return new(
            int.Parse(match.Groups["X"].Value),
            int.Parse(match.Groups["Y"].Value));
    }

    public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

internal class SizeConverter : JsonConverter<Size>
{
    public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()
            ?? throw new ArgumentNullException(nameof(reader));

        var match = Regex.Match(value, @"{\s*Width=(?<Width>\d+),\s*Height=(?<Height>\d+)\s*}");

        return new(
            int.Parse(match.Groups["Width"].Value),
            int.Parse(match.Groups["Height"].Value));
    }

    public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

internal class EnumConverter<T> : JsonConverter<T> where T : Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()
            ?? throw new ArgumentNullException(nameof(reader));

        return (T)Enum.Parse(typeof(T), value);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

using System.Globalization;

namespace AppTodoPro.Converters;

public class RatioToWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var ratio = value is double d ? d : 0;
        var maxWidth = 220.0;
        if (parameter is string raw && double.TryParse(raw, out var parsed))
        {
            maxWidth = parsed;
        }

        return Math.Max(4, ratio * maxWidth);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

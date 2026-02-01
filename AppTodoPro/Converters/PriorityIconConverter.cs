using System.Globalization;

namespace AppTodoPro.Converters;

public class PriorityIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var priority = value as int? ?? 1;
        return priority switch
        {
            0 => "priority_low.svg",
            2 => "priority_high.svg",
            _ => "priority_medium.svg"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

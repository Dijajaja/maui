using System.Globalization;
using AppTodoPro.Services;
using Microsoft.Maui.Graphics;

namespace AppTodoPro.Converters;

public class CategoryColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var category = value?.ToString() ?? string.Empty;
        var hex = CategoryService.GetColorForCategory(category);
        return Color.FromArgb(hex);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

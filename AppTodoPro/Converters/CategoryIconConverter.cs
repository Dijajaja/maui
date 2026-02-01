using System.Globalization;

namespace AppTodoPro.Converters;

public class CategoryIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var category = value?.ToString() ?? string.Empty;
        return AppTodoPro.Services.CategoryService.GetIconForCategory(category);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

using System.Text.Json;
using Microsoft.Maui.Storage;

namespace AppTodoPro.Services;

public class CategoryService
{
    private const string CategoriesPreferenceKey = "custom_categories";
    private const string CategoryIconsPreferenceKey = "category_icons";
    private const string CategoryColorsPreferenceKey = "category_colors";
    private static readonly IReadOnlyList<string> DefaultCategories = new[]
    {
        "Général",
        "Travail",
        "Personnel",
        "Courses",
        "Santé"
    };

    public List<string> GetCategories()
    {
        var stored = Preferences.Get(CategoriesPreferenceKey, string.Empty);
        if (string.IsNullOrWhiteSpace(stored))
        {
            return DefaultCategories.ToList();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(stored);
            if (parsed is null || parsed.Count == 0)
            {
                return DefaultCategories.ToList();
            }

            return parsed
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return DefaultCategories.ToList();
        }
    }

    public List<string> AddCategory(string name)
    {
        var categories = GetCategories();
        if (categories.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            return categories;
        }

        categories.Add(name);
        SaveCategories(categories);
        return categories;
    }

    public void SaveCategories(IEnumerable<string> categories)
    {
        var payload = JsonSerializer.Serialize(categories.ToList());
        Preferences.Set(CategoriesPreferenceKey, payload);
    }

    public Dictionary<string, string> GetCategoryIcons()
    {
        var stored = Preferences.Get(CategoryIconsPreferenceKey, string.Empty);
        if (string.IsNullOrWhiteSpace(stored))
        {
            return GetDefaultIcons();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(stored);
            if (parsed is null || parsed.Count == 0)
            {
                return GetDefaultIcons();
            }

            return parsed;
        }
        catch
        {
            return GetDefaultIcons();
        }
    }

    public void SetCategoryIcon(string category, string iconKey)
    {
        var icons = GetCategoryIcons();
        icons[category] = iconKey;
        SaveCategoryIcons(icons);
    }

    public Dictionary<string, string> GetCategoryColors()
    {
        var stored = Preferences.Get(CategoryColorsPreferenceKey, string.Empty);
        if (string.IsNullOrWhiteSpace(stored))
        {
            return GetDefaultColors();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(stored);
            if (parsed is null || parsed.Count == 0)
            {
                return GetDefaultColors();
            }

            return parsed;
        }
        catch
        {
            return GetDefaultColors();
        }
    }

    public void SetCategoryColor(string category, string colorHex)
    {
        var colors = GetCategoryColors();
        colors[category] = colorHex;
        SaveCategoryColors(colors);
    }

    public static string GetColorForCategory(string category)
    {
        var stored = Preferences.Get(CategoryColorsPreferenceKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(stored))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(stored);
                if (parsed is not null && parsed.TryGetValue(category, out var color))
                {
                    return color;
                }
            }
            catch
            {
            }
        }

        return GetDefaultColor(category);
    }

    public static string GetIconForCategory(string category)
    {
        var stored = Preferences.Get(CategoryIconsPreferenceKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(stored))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(stored);
                if (parsed is not null && parsed.TryGetValue(category, out var icon))
                {
                    return icon;
                }
            }
            catch
            {
            }
        }

        return GetDefaultIcon(category);
    }

    private static Dictionary<string, string> GetDefaultIcons()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Général"] = "category_general.svg",
            ["Travail"] = "category_work.svg",
            ["Personnel"] = "category_personal.svg",
            ["Courses"] = "category_shopping.svg",
            ["Santé"] = "category_health.svg"
        };
    }

    private static string GetDefaultIcon(string category)
    {
        return category.ToLowerInvariant() switch
        {
            "travail" => "category_work.svg",
            "personnel" => "category_personal.svg",
            "courses" => "category_shopping.svg",
            "santé" => "category_health.svg",
            _ => "category_general.svg"
        };
    }

    private static void SaveCategoryIcons(Dictionary<string, string> icons)
    {
        var payload = JsonSerializer.Serialize(icons);
        Preferences.Set(CategoryIconsPreferenceKey, payload);
    }

    private static Dictionary<string, string> GetDefaultColors()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Général"] = "#5C6BC0",
            ["Travail"] = "#3949AB",
            ["Personnel"] = "#8E24AA",
            ["Courses"] = "#00897B",
            ["Santé"] = "#43A047"
        };
    }

    private static string GetDefaultColor(string category)
    {
        return category.ToLowerInvariant() switch
        {
            "travail" => "#3949AB",
            "personnel" => "#8E24AA",
            "courses" => "#00897B",
            "santé" => "#43A047",
            _ => "#5C6BC0"
        };
    }

    private static void SaveCategoryColors(Dictionary<string, string> colors)
    {
        var payload = JsonSerializer.Serialize(colors);
        Preferences.Set(CategoryColorsPreferenceKey, payload);
    }
}

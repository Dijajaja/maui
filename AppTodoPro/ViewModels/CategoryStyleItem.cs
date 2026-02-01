using System.ComponentModel;
using System.Runtime.CompilerServices;
using AppTodoPro.Services;

namespace AppTodoPro.ViewModels;

public class CategoryStyleItem : INotifyPropertyChanged
{
    private readonly CategoryService categoryService;
    private CategoryIconOption? selectedIcon;
    private CategoryColorOption? selectedColor;

    public CategoryStyleItem(
        string name,
        CategoryIconOption? icon,
        CategoryColorOption? color,
        CategoryService categoryService)
    {
        Name = name;
        selectedIcon = icon;
        selectedColor = color;
        this.categoryService = categoryService;
    }

    public string Name { get; }

    public CategoryIconOption? SelectedIcon
    {
        get => selectedIcon;
        set
        {
            if (selectedIcon == value)
            {
                return;
            }

            selectedIcon = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IconSource));

            if (selectedIcon is not null)
            {
                categoryService.SetCategoryIcon(Name, selectedIcon.IconSource);
            }
        }
    }

    public CategoryColorOption? SelectedColor
    {
        get => selectedColor;
        set
        {
            if (selectedColor == value)
            {
                return;
            }

            selectedColor = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ColorHex));

            if (selectedColor is not null)
            {
                categoryService.SetCategoryColor(Name, selectedColor.Hex);
            }
        }
    }

    public string IconSource => selectedIcon?.IconSource ?? "category_general.svg";

    public string ColorHex => selectedColor?.Hex ?? "#5C6BC0";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

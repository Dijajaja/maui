using System.ComponentModel;
using System.Runtime.CompilerServices;
using AppTodoPro.Services;

namespace AppTodoPro.ViewModels;

public class CategoryIconItem : INotifyPropertyChanged
{
    private readonly CategoryService categoryService;
    private CategoryIconOption? selectedIcon;

    public CategoryIconItem(string name, CategoryIconOption? selectedIcon, CategoryService categoryService)
    {
        Name = name;
        this.selectedIcon = selectedIcon;
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

    public string IconSource => selectedIcon?.IconSource ?? "category_general.svg";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

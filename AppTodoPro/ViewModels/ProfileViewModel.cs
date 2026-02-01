using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AppTodoPro.Services;
using Microsoft.Maui.Storage;

namespace AppTodoPro.ViewModels;

public class ProfileViewModel : INotifyPropertyChanged
{
    private const string ThemePreferenceKey = "theme_preference";
    private readonly AuthService authService;
    private readonly CategoryService categoryService;
    private string email = "—";
    private string displayName = "Utilisateur";
    private string editableName = string.Empty;
    private string? nameError;
    private bool isDarkMode;
    private bool isBusy;

    public ProfileViewModel(AuthService authService, CategoryService categoryService)
    {
        this.authService = authService;
        this.categoryService = categoryService;
        SignOutCommand = new Command(SignOut);
        ToggleThemeCommand = new Command(ToggleTheme);
        SaveDisplayNameCommand = new Command(async () => await SaveDisplayNameAsync(), CanSaveDisplayName);
        RefreshCategories();

        LoadThemePreference();
    }

    public string Email
    {
        get => email;
        private set
        {
            if (email == value)
            {
                return;
            }

            email = value;
            OnPropertyChanged();
        }
    }

    public string DisplayName
    {
        get => displayName;
        private set
        {
            if (displayName == value)
            {
                return;
            }

            displayName = value;
            OnPropertyChanged();
        }
    }

    public string LiveDisplayName => string.IsNullOrWhiteSpace(EditableName) ? "Utilisateur" : EditableName.Trim();

    public string EditableName
    {
        get => editableName;
        set
        {
            if (editableName == value)
            {
                return;
            }

            editableName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LiveDisplayName));
            SaveDisplayNameCommand.ChangeCanExecute();
        }
    }

    public string? NameError
    {
        get => nameError;
        private set
        {
            if (nameError == value)
            {
                return;
            }

            nameError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasNameError));
        }
    }

    public bool HasNameError => !string.IsNullOrWhiteSpace(NameError);


    public bool IsDarkMode
    {
        get => isDarkMode;
        set
        {
            if (isDarkMode == value)
            {
                return;
            }

            isDarkMode = value;
            OnPropertyChanged();
            ApplyTheme();
        }
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (isBusy == value)
            {
                return;
            }

            isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotBusy));
            SaveDisplayNameCommand.ChangeCanExecute();
        }
    }

    public bool IsNotBusy => !IsBusy;

    public Command SignOutCommand { get; }

    public Command ToggleThemeCommand { get; }

    public Command SaveDisplayNameCommand { get; }

    public ObservableCollection<CategoryIconOption> IconOptions { get; } = new()
    {
        new CategoryIconOption { Key = "general", Label = "Général", IconSource = "category_general.svg" },
        new CategoryIconOption { Key = "work", Label = "Travail", IconSource = "category_work.svg" },
        new CategoryIconOption { Key = "personal", Label = "Personnel", IconSource = "category_personal.svg" },
        new CategoryIconOption { Key = "shopping", Label = "Courses", IconSource = "category_shopping.svg" },
        new CategoryIconOption { Key = "health", Label = "Santé", IconSource = "category_health.svg" }
    };

    public ObservableCollection<CategoryColorOption> ColorOptions { get; } = new()
    {
        new CategoryColorOption { Label = "Indigo", Hex = "#3949AB" },
        new CategoryColorOption { Label = "Violet", Hex = "#8E24AA" },
        new CategoryColorOption { Label = "Turquoise", Hex = "#00897B" },
        new CategoryColorOption { Label = "Vert", Hex = "#43A047" },
        new CategoryColorOption { Label = "Orange", Hex = "#FB8C00" },
        new CategoryColorOption { Label = "Rouge", Hex = "#D32F2F" },
        new CategoryColorOption { Label = "Bleu", Hex = "#5C6BC0" }
    };

    public ObservableCollection<CategoryStyleItem> Categories { get; } = new();

    public event EventHandler? SignedOut;

    public event PropertyChangedEventHandler? PropertyChanged;

    public async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var user = await authService.GetCurrentUserAsync();
            Email = user?.Email ?? "—";
            DisplayName = string.IsNullOrWhiteSpace(user?.Name) ? "Utilisateur" : user!.Name;
            EditableName = DisplayName;
            OnPropertyChanged(nameof(LiveDisplayName));
            RefreshCategories();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveDisplayNameAsync()
    {
        if (IsBusy)
        {
            return;
        }

        NameError = null;
        if (string.IsNullOrWhiteSpace(EditableName))
        {
            NameError = "Nom requis.";
            return;
        }

        var trimmed = EditableName.Trim();
        if (string.Equals(trimmed, DisplayName, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            IsBusy = true;
            var saved = await authService.UpdateDisplayNameAsync(trimmed);
            if (!saved)
            {
                NameError = "Impossible de mettre à jour le nom.";
                return;
            }

            DisplayName = trimmed;
            OnPropertyChanged(nameof(LiveDisplayName));
            await ToastService.ShowAsync("Pseudo mis à jour.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSaveDisplayName()
    {
        if (IsBusy)
        {
            return false;
        }

        var trimmed = EditableName.Trim();
        return !string.IsNullOrWhiteSpace(trimmed) && !string.Equals(trimmed, DisplayName, StringComparison.Ordinal);
    }

    private void SignOut()
    {
        authService.SignOut();
        SignedOut?.Invoke(this, EventArgs.Empty);
    }

    private void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
    }

    private void ApplyTheme()
    {
        Preferences.Set(ThemePreferenceKey, IsDarkMode ? "dark" : "light");
        if (Application.Current is not null)
        {
            Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        }
    }

    private void LoadThemePreference()
    {
        var stored = Preferences.Get(ThemePreferenceKey, "system");
        if (stored == "dark")
        {
            isDarkMode = true;
        }
        else if (stored == "light")
        {
            isDarkMode = false;
        }
        else
        {
            isDarkMode = Application.Current?.UserAppTheme == AppTheme.Dark;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void RefreshCategories()
    {
        Categories.Clear();
        var categories = categoryService.GetCategories();
        var icons = categoryService.GetCategoryIcons();
        var colors = categoryService.GetCategoryColors();

        foreach (var category in categories)
        {
            icons.TryGetValue(category, out var icon);
            colors.TryGetValue(category, out var color);
            var option = IconOptions.FirstOrDefault(item => item.IconSource == icon)
                ?? IconOptions.FirstOrDefault(item => item.Key == "general");
            var colorOption = ColorOptions.FirstOrDefault(item => item.Hex == color)
                ?? ColorOptions.FirstOrDefault(item => item.Label == "Bleu");
            Categories.Add(new CategoryStyleItem(category, option, colorOption, categoryService));
        }
    }
}

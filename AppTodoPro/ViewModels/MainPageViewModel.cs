using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AppTodoPro.Models;
using AppTodoPro.Services;
using AppTodoPro;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace AppTodoPro.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly TodoRepository repository;
    private readonly AuthService authService;
    private readonly CategoryService categoryService;
    private readonly ObservableCollection<TodoItem> items;
    private readonly ObservableCollection<TodoItem> filteredItems;
    private readonly ObservableCollection<string> categoryOptions;
    private readonly ObservableCollection<string> filterCategoryOptions;
    private string? newTitle;
    private string? newTags;
    private string? searchText;
    private string? tagFilterText;
    private string? suggestionText;
    private int selectedCategoryIndex;
    private int selectedPriorityIndex = 1;
    private bool hasDueDate;
    private DateTime dueDate = DateTime.Today;
    private int filterCategoryIndex;
    private int filterPriorityIndex;
    private int filterStatusIndex;
    private int sortOptionIndex = 1;
    private int? currentUserId;
    private bool isBusy;
    private bool isApplyingFilters;
    private bool pendingFilterApply;

    public MainPageViewModel(
        TodoRepository repository,
        AuthService authService,
        CategoryService categoryService)
    {
        this.repository = repository;
        this.authService = authService;
        this.categoryService = categoryService;
        items = new ObservableCollection<TodoItem>();
        filteredItems = new ObservableCollection<TodoItem>();
        Items = new ReadOnlyObservableCollection<TodoItem>(items);
        categoryOptions = new ObservableCollection<string>(categoryService.GetCategories());
        filterCategoryOptions = new ObservableCollection<string>();
        RefreshFilterCategories();

        AddCommand = new Command(async () => await AddItemAsync());
        DeleteCommand = new Command<TodoItem>(async item => await DeleteItemAsync(item));
        ClearDoneCommand = new Command(async () => await ClearDoneAsync());
        EditCommand = new Command<TodoItem>(async item => await EditItemAsync(item));
        AddCategoryCommand = new Command(async () => await AddCategoryAsync());
        MarkDoneCommand = new Command<TodoItem>(async item => await MarkDoneAsync(item));
        SignOutCommand = new Command(SignOut);
    }

    public ReadOnlyObservableCollection<TodoItem> Items { get; }

    public ObservableCollection<TodoItem> FilteredItems => filteredItems;
    public string? SearchText
    {
        get => searchText;
        set
        {
            if (searchText == value)
            {
                return;
            }

            searchText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanReorder));
            RequestApplyFilters();
        }
    }


    public string? NewTitle
    {
        get => newTitle;
        set
        {
            if (newTitle == value)
            {
                return;
            }

            newTitle = value;
            OnPropertyChanged();
            ApplySmartSuggestions();
        }
    }

    public string? NewTags
    {
        get => newTags;
        set
        {
            if (newTags == value)
            {
                return;
            }

            newTags = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> CategoryOptions => categoryOptions;

    public int SelectedCategoryIndex
    {
        get => selectedCategoryIndex;
        set
        {
            if (selectedCategoryIndex == value)
            {
                return;
            }

            selectedCategoryIndex = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<string> PriorityOptions { get; } = new[]
    {
        "Basse",
        "Normale",
        "Haute"
    };

    public int SelectedPriorityIndex
    {
        get => selectedPriorityIndex;
        set
        {
            if (selectedPriorityIndex == value)
            {
                return;
            }

            selectedPriorityIndex = value;
            OnPropertyChanged();
        }
    }

    public bool HasDueDate
    {
        get => hasDueDate;
        set
        {
            if (hasDueDate == value)
            {
                return;
            }

            hasDueDate = value;
            OnPropertyChanged();
        }
    }

    public DateTime DueDate
    {
        get => dueDate;
        set
        {
            if (dueDate == value)
            {
                return;
            }

            dueDate = value;
            OnPropertyChanged();
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
        }
    }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<string> FilterCategoryOptions => filterCategoryOptions;

    public IReadOnlyList<string> FilterPriorityOptions { get; } = new[]
    {
        "Toutes",
        "Basse",
        "Normale",
        "Haute"
    };

    public IReadOnlyList<string> FilterStatusOptions { get; } = new[]
    {
        "Toutes",
        "En cours",
        "Terminées"
    };

    public IReadOnlyList<string> SortOptions { get; } = new[]
    {
        "Manuel",
        "Récent",
        "Priorité",
        "Catégorie",
        "Échéance"
    };

    public int FilterCategoryIndex
    {
        get => filterCategoryIndex;
        set
        {
            if (filterCategoryIndex == value)
            {
                return;
            }

            filterCategoryIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanReorder));
            RequestApplyFilters();
        }
    }

    public int FilterPriorityIndex
    {
        get => filterPriorityIndex;
        set
        {
            if (filterPriorityIndex == value)
            {
                return;
            }

            filterPriorityIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanReorder));
            RequestApplyFilters();
        }
    }

    public int SortOptionIndex
    {
        get => sortOptionIndex;
        set
        {
            if (sortOptionIndex == value)
            {
                return;
            }

            sortOptionIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanReorder));
            RequestApplyFilters();
        }
    }

    public bool IsEmpty => FilteredItems.Count == 0;

    public string CountLabel => $"{FilteredItems.Count} tâche(s)";

    public string? TagFilterText
    {
        get => tagFilterText;
        set
        {
            if (tagFilterText == value)
            {
                return;
            }

            tagFilterText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanReorder));
            RequestApplyFilters();
        }
    }

    public string? SuggestionText
    {
        get => suggestionText;
        private set
        {
            if (suggestionText == value)
            {
                return;
            }

            suggestionText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSuggestion));
        }
    }

    public bool HasSuggestion => !string.IsNullOrWhiteSpace(SuggestionText);

    public int FilterStatusIndex
    {
        get => filterStatusIndex;
        set
        {
            if (filterStatusIndex == value)
            {
                return;
            }

            filterStatusIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanReorder));
            RequestApplyFilters();
        }
    }

    public bool CanReorder =>
        SortOptionIndex == 0
        && FilterCategoryIndex == 0
        && FilterPriorityIndex == 0
        && FilterStatusIndex == 0
        && string.IsNullOrWhiteSpace(SearchText)
        && string.IsNullOrWhiteSpace(TagFilterText);

    public Command AddCommand { get; }

    public Command<TodoItem> DeleteCommand { get; }

    public Command ClearDoneCommand { get; }

    public Command<TodoItem> EditCommand { get; }

    public Command AddCategoryCommand { get; }

    public Command<TodoItem> MarkDoneCommand { get; }

    public Command SignOutCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? SignedOut;

    public async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            currentUserId = authService.CurrentUserId;
            items.Clear();

            if (currentUserId is null)
            {
                RequestApplyFilters();
                return;
            }

            var loadedItems = await repository.GetItemsAsync(currentUserId.Value);
            if (loadedItems.Any(item => item.OrderIndex == 0))
            {
                var ordered = loadedItems.OrderBy(item => item.CreatedAt).ToList();
                for (var i = 0; i < ordered.Count; i++)
                {
                    ordered[i].OrderIndex = i + 1;
                }

                await repository.UpdateOrderAsync(ordered);
            }

            foreach (var item in loadedItems)
            {
                AttachItem(item);
                this.items.Add(item);
            }

            RequestApplyFilters();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddItemAsync()
    {
        if (IsBusy || currentUserId is null || string.IsNullOrWhiteSpace(NewTitle))
        {
            return;
        }

        var title = NewTitle.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        var category = CategoryOptions.ElementAtOrDefault(SelectedCategoryIndex) ?? "Général";

        var item = new TodoItem
        {
            Title = title,
            Category = category,
            Priority = SelectedPriorityIndex,
            DueDate = HasDueDate ? DueDate.Date : null,
            IsDone = false,
            UserId = currentUserId.Value,
            TagsRaw = NewTags ?? string.Empty
        };

        await repository.AddItemAsync(item);
        AttachItem(item);
        items.Insert(0, item);
        NewTitle = string.Empty;
        NewTags = string.Empty;
        SelectedCategoryIndex = 0;
        SelectedPriorityIndex = 1;
        HasDueDate = false;
        DueDate = DateTime.Today;
        RequestApplyFilters();
    }

    private async Task DeleteItemAsync(TodoItem? item)
    {
        if (IsBusy || item is null)
        {
            return;
        }

        DetachItem(item);
        items.Remove(item);
        await repository.DeleteItemAsync(item);
        RequestApplyFilters();
    }

    private async Task MarkDoneAsync(TodoItem? item)
    {
        if (item is null)
        {
            return;
        }

        item.IsDone = true;
        await repository.UpdateItemAsync(item);
        RequestApplyFilters();
    }

    private async Task ClearDoneAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var toRemove = items.Where(item => item.IsDone).ToList();
        foreach (var item in toRemove)
        {
            DetachItem(item);
            items.Remove(item);
        }

        if (currentUserId is not null)
        {
            await repository.ClearDoneAsync(currentUserId.Value);
        }

        RequestApplyFilters();
    }

    private async Task EditItemAsync(TodoItem? item)
    {
        var page = GetActivePage();
        if (item is null || page is null)
        {
            return;
        }

        await page.Navigation.PushModalAsync(new EditTaskPage(item));
    }

    private async Task AddCategoryAsync()
    {
        var page = GetActivePage();
        if (page is null)
        {
            return;
        }

        var result = await page.DisplayPromptAsync(
            "Nouvelle catégorie",
            "Nom de la catégorie");

        if (string.IsNullOrWhiteSpace(result))
        {
            return;
        }

        var name = result.Trim();
        var updated = categoryService.AddCategory(name);
        categoryOptions.Clear();
        foreach (var category in updated)
        {
            categoryOptions.Add(category);
        }

        SaveCategories();
        RefreshFilterCategories();
    }

    private void AttachItem(TodoItem item)
    {
        item.PropertyChanged += OnItemPropertyChanged;
    }

    private void DetachItem(TodoItem item)
    {
        item.PropertyChanged -= OnItemPropertyChanged;
    }

    private async void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not TodoItem item)
        {
            return;
        }

        if (e.PropertyName == nameof(TodoItem.IsDone)
            || e.PropertyName == nameof(TodoItem.Title)
            || e.PropertyName == nameof(TodoItem.Category)
            || e.PropertyName == nameof(TodoItem.Priority)
            || e.PropertyName == nameof(TodoItem.DueDate))
        {
            await repository.UpdateItemAsync(item);
            RequestApplyFilters();
        }
    }

    private void SignOut()
    {
        authService.SignOut();
        SignedOut?.Invoke(this, EventArgs.Empty);
    }

    private void ApplySmartSuggestions()
    {
        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            SuggestionText = null;
            return;
        }

        var title = NewTitle.ToLowerInvariant();
        string? suggestedCategory = null;
        int? suggestedPriority = null;
        var suggestedTags = new List<string>();

        if (title.Contains("réviser") || title.Contains("examen") || title.Contains("étude"))
        {
            suggestedCategory = "Personnel";
            suggestedTags.Add("#études");
            suggestedTags.Add("#examen");
        }
        else if (title.Contains("réunion") || title.Contains("client") || title.Contains("rapport"))
        {
            suggestedCategory = "Travail";
            suggestedTags.Add("#travail");
        }
        else if (title.Contains("courses") || title.Contains("acheter"))
        {
            suggestedCategory = "Courses";
            suggestedTags.Add("#courses");
        }

        if (title.Contains("urgent") || title.Contains("aujourd"))
        {
            suggestedPriority = 2;
            suggestedTags.Add("#urgent");
        }
        else if (title.Contains("demain"))
        {
            suggestedPriority = 1;
        }

        var messages = new List<string>();
        if (!string.IsNullOrWhiteSpace(suggestedCategory))
        {
            var index = CategoryOptions.IndexOf(suggestedCategory);
            if (index >= 0)
            {
                SelectedCategoryIndex = index;
                messages.Add($"Catégorie suggérée: {suggestedCategory}");
            }
        }

        if (suggestedPriority.HasValue)
        {
            SelectedPriorityIndex = suggestedPriority.Value;
            messages.Add($"Priorité suggérée: {PriorityOptions[SelectedPriorityIndex]}");
        }

        if (suggestedTags.Count > 0)
        {
            var existing = (NewTags ?? string.Empty)
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(tag => tag.StartsWith('#') ? tag : $"#{tag}")
                .ToList();

            foreach (var tag in suggestedTags)
            {
                if (!existing.Contains(tag, StringComparer.OrdinalIgnoreCase))
                {
                    existing.Add(tag);
                }
            }

            NewTags = string.Join(" ", existing);
            messages.Add("Tags suggérés ajoutés");
        }

        SuggestionText = messages.Count == 0 ? null : string.Join(" • ", messages);
    }

    private void RequestApplyFilters()
    {
        if (isApplyingFilters)
        {
            pendingFilterApply = true;
            return;
        }

        MainThread.BeginInvokeOnMainThread(ApplyFiltersSafe);
    }

    private void ApplyFiltersSafe()
    {
        if (isApplyingFilters)
        {
            pendingFilterApply = true;
            return;
        }

        isApplyingFilters = true;
        try
        {
            ApplyFiltersCore();
        }
        finally
        {
            isApplyingFilters = false;
            if (pendingFilterApply)
            {
                pendingFilterApply = false;
                RequestApplyFilters();
            }
        }
    }

    private void ApplyFiltersCore()
    {
        var query = items.AsEnumerable();

        if (FilterCategoryIndex > 0)
        {
            var category = FilterCategoryOptions.ElementAtOrDefault(FilterCategoryIndex);
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(item => item.Category == category);
            }
        }

        if (FilterPriorityIndex > 0)
        {
            var priority = FilterPriorityIndex - 1;
            query = query.Where(item => item.Priority == priority);
        }

        if (FilterStatusIndex == 1)
        {
            query = query.Where(item => !item.IsDone);
        }
        else if (FilterStatusIndex == 2)
        {
            query = query.Where(item => item.IsDone);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            query = query.Where(item =>
                item.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                || item.Category.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(TagFilterText))
        {
            var tags = TagFilterText
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(tag => tag.StartsWith('#') ? tag : $"#{tag}")
                .ToList();

            query = query.Where(item => tags.All(tag => item.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
        }

        query = SortOptionIndex switch
        {
            2 => query.OrderByDescending(item => item.Priority).ThenByDescending(item => item.CreatedAt),
            3 => query.OrderBy(item => item.Category).ThenByDescending(item => item.CreatedAt),
            4 => query.OrderBy(item => item.DueDate ?? DateTime.MaxValue).ThenByDescending(item => item.CreatedAt),
            0 => query.OrderBy(item => item.OrderIndex),
            _ => query.OrderByDescending(item => item.CreatedAt)
        };

        filteredItems.Clear();
        foreach (var item in query)
        {
            filteredItems.Add(item);
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(CountLabel));
    }

    public async Task ReorderItemsAsync(int oldIndex, int newIndex)
    {
        if (!CanReorder || oldIndex == newIndex)
        {
            return;
        }

        if (oldIndex < 0 || newIndex < 0 || oldIndex >= filteredItems.Count || newIndex >= filteredItems.Count)
        {
            return;
        }

        var item = filteredItems[oldIndex];
        filteredItems.RemoveAt(oldIndex);
        filteredItems.Insert(newIndex, item);

        items.Clear();
        for (var i = 0; i < filteredItems.Count; i++)
        {
            filteredItems[i].OrderIndex = i + 1;
            items.Add(filteredItems[i]);
        }

        await repository.UpdateOrderAsync(items);
        RequestApplyFilters();
    }

    public async Task PersistOrderAsync()
    {
        if (!CanReorder || filteredItems.Count == 0)
        {
            return;
        }

        items.Clear();
        for (var i = 0; i < filteredItems.Count; i++)
        {
            filteredItems[i].OrderIndex = i + 1;
            items.Add(filteredItems[i]);
        }

        await repository.UpdateOrderAsync(items);
        RequestApplyFilters();
    }

    private void RefreshFilterCategories()
    {
        filterCategoryOptions.Clear();
        filterCategoryOptions.Add("Toutes");
        foreach (var category in categoryOptions)
        {
            filterCategoryOptions.Add(category);
        }
    }

    private void SaveCategories()
    {
        categoryService.SaveCategories(categoryOptions.ToList());
    }

    private static Page? GetActivePage()
    {
        return Application.Current?.Windows.FirstOrDefault()?.Page;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AppTodoPro.Models;
using AppTodoPro.Services;
using System.Text.Json;
using Microsoft.Maui.Controls;

namespace AppTodoPro.ViewModels;

public class EditTaskViewModel : INotifyPropertyChanged
{
    private readonly TodoRepository repository;
    private readonly CategoryService categoryService;
    private TodoItem? item;
    private string title = string.Empty;
    private bool isDone;
    private bool hasDueDate;
    private DateTime dueDate = DateTime.Today;
    private int selectedCategoryIndex;
    private int selectedPriorityIndex = 1;
    private string tagsRaw = string.Empty;

    public EditTaskViewModel(TodoRepository repository, CategoryService categoryService)
    {
        this.repository = repository;
        this.categoryService = categoryService;
        CategoryOptions = new ObservableCollection<string>(categoryService.GetCategories());
        SaveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
        CancelCommand = new Command(Cancel);
        AddSubtaskCommand = new Command(async () => await AddSubtaskAsync());
        RemoveSubtaskCommand = new Command<SubtaskItem>(RemoveSubtask);
    }

    public ObservableCollection<string> CategoryOptions { get; }

    public ObservableCollection<SubtaskItem> Subtasks { get; } = new();

    public IReadOnlyList<string> PriorityOptions { get; } = new[]
    {
        "Basse",
        "Normale",
        "Haute"
    };

    public string Title
    {
        get => title;
        set
        {
            if (title == value)
            {
                return;
            }

            title = value;
            OnPropertyChanged();
        }
    }

    public bool IsDone
    {
        get => isDone;
        set
        {
            if (isDone == value)
            {
                return;
            }

            isDone = value;
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

    public bool IsBusy { get; private set; }

    public string TagsRaw
    {
        get => tagsRaw;
        set
        {
            if (tagsRaw == value)
            {
                return;
            }

            tagsRaw = value;
            OnPropertyChanged();
        }
    }

    public Command SaveCommand { get; }

    public Command CancelCommand { get; }

    public Command AddSubtaskCommand { get; }

    public Command<SubtaskItem> RemoveSubtaskCommand { get; }

    public event EventHandler? CloseRequested;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Initialize(TodoItem source)
    {
        item = source;
        CategoryOptions.Clear();
        foreach (var category in categoryService.GetCategories())
        {
            CategoryOptions.Add(category);
        }

        Title = source.Title;
        IsDone = source.IsDone;
        HasDueDate = source.DueDate.HasValue;
        DueDate = source.DueDate ?? DateTime.Today;
        SelectedPriorityIndex = source.Priority;
        SelectedCategoryIndex = CategoryOptions.IndexOf(source.Category);
        if (SelectedCategoryIndex < 0)
        {
            CategoryOptions.Add(source.Category);
            SelectedCategoryIndex = CategoryOptions.Count - 1;
        }

        TagsRaw = source.TagsRaw;
        LoadSubtasks(source.SubtasksJson);

    }

    private async Task SaveAsync()
    {
        if (item is null || string.IsNullOrWhiteSpace(Title))
        {
            return;
        }

        try
        {
            IsBusy = true;
            SaveCommand.ChangeCanExecute();

            item.Title = Title.Trim();
            item.IsDone = IsDone;
            item.Priority = SelectedPriorityIndex;
            item.Category = CategoryOptions.ElementAtOrDefault(SelectedCategoryIndex) ?? "Général";
            item.DueDate = HasDueDate ? DueDate.Date : null;
            item.TagsRaw = TagsRaw;
            item.SubtasksJson = SerializeSubtasks();

            await repository.UpdateItemAsync(item);
        }
        finally
        {
            IsBusy = false;
            SaveCommand.ChangeCanExecute();
        }

        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void Cancel()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private async Task AddSubtaskAsync()
    {
        if (Application.Current?.Windows.FirstOrDefault()?.Page is not Page page)
        {
            return;
        }

        var title = await page.DisplayPromptAsync("Sous-tâche", "Titre de la sous-tâche");
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        Subtasks.Add(new SubtaskItem { Title = title.Trim(), IsDone = false });
    }

    private void RemoveSubtask(SubtaskItem? subtask)
    {
        if (subtask is null)
        {
            return;
        }

        Subtasks.Remove(subtask);
    }

    private void LoadSubtasks(string? json)
    {
        Subtasks.Clear();
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<SubtaskItem>>(json);
            if (items is null)
            {
                return;
            }

            foreach (var item in items)
            {
                Subtasks.Add(item);
            }
        }
        catch
        {
        }
    }

    private string SerializeSubtasks()
    {
        return JsonSerializer.Serialize(Subtasks.ToList());
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

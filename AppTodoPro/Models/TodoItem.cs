using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace AppTodoPro.Models;

public class TodoItem : INotifyPropertyChanged
{
    private string title = string.Empty;
    private bool isDone;
    private string category = "Général";
    private int priority = 1;
    private DateTime? dueDate;
    private string tagsRaw = string.Empty;
    private string subtasksJson = "[]";

    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int OrderIndex { get; set; }

    [MaxLength(200)]
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

    [MaxLength(100)]
    public string Category
    {
        get => category;
        set
        {
            if (category == value)
            {
                return;
            }

            category = value;
            OnPropertyChanged();
        }
    }

    public int Priority
    {
        get => priority;
        set
        {
            if (priority == value)
            {
                return;
            }

            priority = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PriorityLabel));
        }
    }

    public DateTime? DueDate
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
            OnPropertyChanged(nameof(DueLabel));
        }
    }

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
            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(HasTags));
        }
    }

    public string SubtasksJson
    {
        get => subtasksJson;
        set
        {
            if (subtasksJson == value)
            {
                return;
            }

            subtasksJson = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SubtaskTotal));
            OnPropertyChanged(nameof(SubtaskDone));
            OnPropertyChanged(nameof(SubtaskProgress));
            OnPropertyChanged(nameof(SubtaskLabel));
            OnPropertyChanged(nameof(HasSubtasks));
        }
    }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [Ignore]
    public string PriorityLabel => Priority switch
    {
        0 => "Basse",
        2 => "Haute",
        _ => "Normale"
    };

    [Ignore]
    public string CreatedLabel => $"Créée le {CreatedAt:dd/MM/yyyy}";

    [Ignore]
    public string DueLabel => DueDate.HasValue
        ? $"Échéance: {DueDate:dd/MM/yyyy}"
        : "Sans échéance";

    [Ignore]
    public IReadOnlyList<string> Tags => TagsRaw
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(tag => !string.IsNullOrWhiteSpace(tag))
        .Select(tag => tag.StartsWith('#') ? tag : $"#{tag}")
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    [Ignore]
    public bool HasTags => Tags.Count > 0;

    [Ignore]
    public int SubtaskTotal => GetSubtasks().Count;

    [Ignore]
    public int SubtaskDone => GetSubtasks().Count(item => item.IsDone);

    [Ignore]
    public double SubtaskProgress => SubtaskTotal == 0 ? 0 : (double)SubtaskDone / SubtaskTotal;

    [Ignore]
    public string SubtaskLabel => SubtaskTotal == 0 ? "Aucune sous-tâche" : $"{SubtaskDone}/{SubtaskTotal} terminées";

    [Ignore]
    public bool HasSubtasks => SubtaskTotal > 0;

    private List<SubtaskItem> GetSubtasks()
    {
        if (string.IsNullOrWhiteSpace(SubtasksJson))
        {
            return new List<SubtaskItem>();
        }

        try
        {
            var items = System.Text.Json.JsonSerializer.Deserialize<List<SubtaskItem>>(SubtasksJson);
            return items ?? new List<SubtaskItem>();
        }
        catch
        {
            return new List<SubtaskItem>();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

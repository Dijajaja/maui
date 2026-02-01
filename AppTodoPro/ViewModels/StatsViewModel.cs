using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AppTodoPro.Models;
using AppTodoPro.Services;

namespace AppTodoPro.ViewModels;

public class StatsViewModel : INotifyPropertyChanged
{
    private readonly TodoRepository repository;
    private readonly AuthService authService;
    private bool isBusy;
    private int totalCount;
    private int doneCount;
    private int pendingCount;
    private int highCount;
    private int mediumCount;
    private int lowCount;
    private int maxDailyCount;

    public StatsViewModel(TodoRepository repository, AuthService authService)
    {
        this.repository = repository;
        this.authService = authService;
        CategoryStats = new ObservableCollection<StatItem>();
        WeeklyTrend = new ObservableCollection<TrendItem>();
    }

    public int TotalCount
    {
        get => totalCount;
        private set
        {
            if (totalCount == value)
            {
                return;
            }

            totalCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DoneRatio));
        }
    }

    public int DoneCount
    {
        get => doneCount;
        private set
        {
            if (doneCount == value)
            {
                return;
            }

            doneCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DoneRatio));
        }
    }

    public int PendingCount
    {
        get => pendingCount;
        private set
        {
            if (pendingCount == value)
            {
                return;
            }

            pendingCount = value;
            OnPropertyChanged();
        }
    }

    public int HighCount
    {
        get => highCount;
        private set
        {
            if (highCount == value)
            {
                return;
            }

            highCount = value;
            OnPropertyChanged();
        }
    }

    public int MediumCount
    {
        get => mediumCount;
        private set
        {
            if (mediumCount == value)
            {
                return;
            }

            mediumCount = value;
            OnPropertyChanged();
        }
    }

    public int LowCount
    {
        get => lowCount;
        private set
        {
            if (lowCount == value)
            {
                return;
            }

            lowCount = value;
            OnPropertyChanged();
        }
    }

    public double DoneRatio => TotalCount == 0 ? 0 : (double)DoneCount / TotalCount;

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
        }
    }

    public ObservableCollection<StatItem> CategoryStats { get; }

    public ObservableCollection<TrendItem> WeeklyTrend { get; }

    public int MaxDailyCount
    {
        get => maxDailyCount;
        private set
        {
            if (maxDailyCount == value)
            {
                return;
            }

            maxDailyCount = value;
            OnPropertyChanged();
        }
    }

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
            var userId = authService.CurrentUserId;
            if (userId is null)
            {
                Reset();
                return;
            }

            var items = await repository.GetItemsAsync(userId.Value);
            BuildStats(items);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BuildStats(List<TodoItem> items)
    {
        TotalCount = items.Count;
        DoneCount = items.Count(item => item.IsDone);
        PendingCount = items.Count(item => !item.IsDone);

        HighCount = items.Count(item => item.Priority == 2);
        MediumCount = items.Count(item => item.Priority == 1);
        LowCount = items.Count(item => item.Priority == 0);

        CategoryStats.Clear();
        foreach (var group in items.GroupBy(item => item.Category).OrderByDescending(group => group.Count()))
        {
            CategoryStats.Add(new StatItem
            {
                Label = group.Key,
                Count = group.Count()
            });
        }

        BuildWeeklyTrend(items);
    }

    private void Reset()
    {
        TotalCount = 0;
        DoneCount = 0;
        PendingCount = 0;
        HighCount = 0;
        MediumCount = 0;
        LowCount = 0;
        CategoryStats.Clear();
        WeeklyTrend.Clear();
    }

    private void BuildWeeklyTrend(List<TodoItem> items)
    {
        WeeklyTrend.Clear();
        var start = DateTime.Today.AddDays(-6);
        var dailyCounts = new List<(DateTime Date, int Count)>();

        for (var i = 0; i < 7; i++)
        {
            var date = start.AddDays(i);
            var count = items.Count(item => item.CreatedAt.Date == date);
            dailyCounts.Add((date, count));
        }

        MaxDailyCount = dailyCounts.Max(entry => entry.Count);
        foreach (var entry in dailyCounts)
        {
            WeeklyTrend.Add(new TrendItem
            {
                Label = entry.Date.ToString("dd/MM"),
                Count = entry.Count,
                Ratio = MaxDailyCount == 0 ? 0 : (double)entry.Count / MaxDailyCount
            });
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

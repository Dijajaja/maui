using AppTodoPro.Models;
using SQLite;
using System.Linq;

namespace AppTodoPro.Services;

public class TodoRepository
{
    private SQLiteAsyncConnection? db;
    private readonly string dbPath;

    public TodoRepository()
    {
        dbPath = Path.Combine(FileSystem.AppDataDirectory, "apptodopro.db3");
    }

    private async Task InitAsync()
    {
        if (db is not null)
        {
            return;
        }

        db = new SQLiteAsyncConnection(dbPath);
        await db.CreateTableAsync<TodoItem>();
        await db.CreateTableAsync<UserAccount>();
        await EnsureColumnsAsync();
    }

    private async Task EnsureColumnsAsync()
    {
        await EnsureColumnAsync("TodoItem", "Category", "TEXT");
        await EnsureColumnAsync("TodoItem", "Priority", "INTEGER");
        await EnsureColumnAsync("TodoItem", "OrderIndex", "INTEGER");
        await EnsureColumnAsync("TodoItem", "DueDate", "TEXT");
        await EnsureColumnAsync("TodoItem", "CreatedAt", "TEXT");
        await EnsureColumnAsync("TodoItem", "UpdatedAt", "TEXT");
        await EnsureColumnAsync("TodoItem", "TagsRaw", "TEXT");
        await EnsureColumnAsync("TodoItem", "SubtasksJson", "TEXT");
    }

    private async Task EnsureColumnAsync(string table, string column, string type)
    {
        var info = await db!.QueryAsync<TableInfo>($"PRAGMA table_info({table})");
        if (info.Any(entry => entry.Name.Equals(column, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        await db.ExecuteAsync($"ALTER TABLE {table} ADD COLUMN {column} {type}");
    }

    public async Task<List<TodoItem>> GetItemsAsync(int userId)
    {
        await InitAsync();
        return await db!.Table<TodoItem>()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.OrderIndex)
            .ThenByDescending(item => item.CreatedAt)
            .ToListAsync();
    }

    public async Task AddItemAsync(TodoItem item)
    {
        await InitAsync();
        item.OrderIndex = await GetNextOrderIndexAsync(item.UserId);
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = item.CreatedAt;
        await db!.InsertAsync(item);
    }

    public async Task UpdateItemAsync(TodoItem item)
    {
        await InitAsync();
        item.UpdatedAt = DateTime.UtcNow;
        await db!.UpdateAsync(item);
    }

    public async Task DeleteItemAsync(TodoItem item)
    {
        await InitAsync();
        await db!.DeleteAsync(item);
    }

    public async Task ClearDoneAsync(int userId)
    {
        await InitAsync();
        var doneItems = await db!.Table<TodoItem>()
            .Where(item => item.IsDone && item.UserId == userId)
            .ToListAsync();

        foreach (var item in doneItems)
        {
            await db.DeleteAsync(item);
        }
    }

    public async Task<int> GetNextOrderIndexAsync(int userId)
    {
        await InitAsync();
        var max = await db!.Table<TodoItem>()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.OrderIndex)
            .FirstOrDefaultAsync();

        return (max?.OrderIndex ?? 0) + 1;
    }

    public async Task UpdateOrderAsync(IEnumerable<TodoItem> orderedItems)
    {
        await InitAsync();
        foreach (var item in orderedItems)
        {
            await db!.UpdateAsync(item);
        }
    }

    private sealed class TableInfo
    {
        public string Name { get; set; } = string.Empty;
    }
}

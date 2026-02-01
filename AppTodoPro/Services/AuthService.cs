using System.Security.Cryptography;
using System.Text;
using AppTodoPro.Models;
using SQLite;

namespace AppTodoPro.Services;

public class AuthService
{
    private const string CurrentUserKey = "current_user_id";
    private SQLiteAsyncConnection? db;
    private readonly string dbPath;

    public AuthService()
    {
        dbPath = Path.Combine(FileSystem.AppDataDirectory, "apptodopro.db3");
    }

    public int? CurrentUserId
    {
        get
        {
            var stored = Preferences.Get(CurrentUserKey, 0);
            return stored == 0 ? null : stored;
        }
    }

    public void SetCurrentUser(int userId)
    {
        Preferences.Set(CurrentUserKey, userId);
    }

    public void SignOut()
    {
        Preferences.Remove(CurrentUserKey);
    }

    public async Task<UserAccount?> RegisterAsync(string email, string password, string name)
    {
        await InitAsync();
        var normalized = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var existing = await db!.Table<UserAccount>()
            .Where(user => user.Email == normalized)
            .FirstOrDefaultAsync();
        if (existing is not null)
        {
            return null;
        }

        var salt = CreateSalt();
        var hash = HashPassword(password, salt);

        var account = new UserAccount
        {
            Email = normalized,
            Name = name.Trim(),
            PasswordSalt = salt,
            PasswordHash = hash,
            CreatedAt = DateTime.UtcNow
        };

        await db.InsertAsync(account);
        return account;
    }

    public async Task<UserAccount?> LoginAsync(string email, string password)
    {
        await InitAsync();
        var normalized = email.Trim().ToLowerInvariant();
        var account = await db!.Table<UserAccount>()
            .Where(user => user.Email == normalized)
            .FirstOrDefaultAsync();
        if (account is null)
        {
            return null;
        }

        var hash = HashPassword(password, account.PasswordSalt);
        return hash == account.PasswordHash ? account : null;
    }

    public async Task<UserAccount?> GetCurrentUserAsync()
    {
        await InitAsync();
        if (CurrentUserId is null)
        {
            return null;
        }

        return await db!.Table<UserAccount>()
            .Where(user => user.Id == CurrentUserId.Value)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateDisplayNameAsync(string name)
    {
        await InitAsync();
        if (CurrentUserId is null)
        {
            return false;
        }

        var trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return false;
        }

        var account = await db!.Table<UserAccount>()
            .Where(user => user.Id == CurrentUserId.Value)
            .FirstOrDefaultAsync();
        if (account is null)
        {
            return false;
        }

        account.Name = trimmed;
        await db.UpdateAsync(account);
        return true;
    }

    private async Task InitAsync()
    {
        if (db is not null)
        {
            return;
        }

        db = new SQLiteAsyncConnection(dbPath);
        await db.CreateTableAsync<UserAccount>();
        await EnsureColumnsAsync();
    }

    private async Task EnsureColumnsAsync()
    {
        await EnsureColumnAsync("UserAccount", "Name", "TEXT");
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

    private sealed class TableInfo
    {
        public string Name { get; set; } = string.Empty;
    }

    private static string CreateSalt()
    {
        var bytes = new byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashPassword(string password, string salt)
    {
        var combined = $"{password}:{salt}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(bytes);
    }
}

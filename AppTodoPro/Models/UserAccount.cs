using SQLite;

namespace AppTodoPro.Models;

public class UserAccount
{
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Unique]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(200)]
    public string PasswordSalt { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

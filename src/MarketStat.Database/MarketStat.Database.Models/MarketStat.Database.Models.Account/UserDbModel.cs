namespace MarketStat.Database.Models.Account;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
public class UserDbModel
{
    public UserDbModel()
    {
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
        IsAdmin = false;
    }

    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [Column("username")]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("full_name")]
    [StringLength(255)]
    public string FullName { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("last_login_at")]
    public DateTimeOffset? LastLoginAt { get; set; }

    [Required]
    [Column("is_admin")]
    public bool IsAdmin { get; set; }
}

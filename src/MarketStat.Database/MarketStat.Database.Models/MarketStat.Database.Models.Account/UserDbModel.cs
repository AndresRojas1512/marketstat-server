using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models.MarketStat.Database.Models.Account;

[Table("users")]
public class UserDbModel
{
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

    [Column("saved_benchmarks_count")]
    public int SavedBenchmarksCount { get; set; }
    
    [Required]
    [Column("is_etl_user")]
    public bool IsEtlUser { get; set; }

    public virtual ICollection<BenchmarkHistoryDbModel> BenchmarkHistories { get; set; }

    public UserDbModel()
    {
        BenchmarkHistories = new List<BenchmarkHistoryDbModel>();
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
        IsEtlUser = false;
    }
}
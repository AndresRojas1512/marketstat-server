using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;

namespace MarketStat.Database.Models;

[Table("dim_date")]
public class DimDateDbModel
{
    [Key]
    [Column("date_id")]
    public int DateId { get; set; }
    
    [Required]
    [Column("full_date")]
    public DateOnly FullDate { get; set; }
    
    [Required]
    [Column("year")]
    public int Year { get; set; }
    
    [Required]
    [Column("quarter")]
    public int Quarter { get; set; }
    
    [Required]
    [Column("month")]
    public int Month { get; set; }
    
    public virtual ICollection<FactSalaryDbModel> FactSalaries { get; set; } = new List<FactSalaryDbModel>();
}
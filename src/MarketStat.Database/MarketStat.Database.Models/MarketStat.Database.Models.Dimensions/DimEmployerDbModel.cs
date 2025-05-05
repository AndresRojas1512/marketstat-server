using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace MarketStat.Database.Models;

[Table("dim_employers")]
public class DimEmployerDbModel
{
    [Key]
    [Column("employer_id")]
    public int EmployerId { get; set; }
    
    [Required]
    [Column("employer_name")]
    [StringLength(255)]
    public string EmployerName { get; set; }
    
    [Column("is_public")]
    public bool IsPublic { get; set; }

    public DimEmployerDbModel(int employerId, string employerName, bool isPublic)
    {
        EmployerId = employerId;
        EmployerName = employerName;
        IsPublic = isPublic;
    }
}
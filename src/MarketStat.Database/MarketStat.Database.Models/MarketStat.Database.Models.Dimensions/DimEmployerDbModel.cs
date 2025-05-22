using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;

namespace MarketStat.Database.Models;

[Table("dim_employer")]
public class DimEmployerDbModel
{
    [Key]
    [Column("employer_id")]
    public int EmployerId { get; set; }

    [Required]
    [Column("employer_name")]
    [StringLength(255)]
    public string EmployerName { get; set; } = string.Empty;
    
    [Column("is_public")]
    public bool IsPublic { get; set; }
    
    public virtual ICollection<DimEmployerIndustryFieldDbModel> EmployerIndustryFields { get; set; } = new List<DimEmployerIndustryFieldDbModel>();
    public virtual ICollection<FactSalaryDbModel> FactSalaries { get; set; } = new List<FactSalaryDbModel>();

    public DimEmployerDbModel()
    {
        FactSalaries = new List<FactSalaryDbModel>();
    }

    public DimEmployerDbModel(int employerId, string employerName, bool isPublic)
    {
        EmployerId = employerId;
        EmployerName = employerName;
        IsPublic = isPublic;
        FactSalaries = new List<FactSalaryDbModel>();
    }
}
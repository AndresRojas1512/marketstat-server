using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;

namespace MarketStat.Database.Models;

[Table("dim_employee")]
public class DimEmployeeDbModel
{
    [Key]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Required]
    [Column("employee_ref_id")]
    [StringLength(255)]
    public string EmployeeRefId { get; set; } = string.Empty;

    [Required]
    [Column("birth_date", TypeName = "date")]
    public DateOnly BirthDate { get; set; }
        
    [Required]
    [Column("career_start_date", TypeName = "date")]
    public DateOnly CareerStartDate { get; set; }

    [Column("gender")]
    [StringLength(50)]
    public string? Gender { get; set; }

    public virtual ICollection<DimEmployeeEducationDbModel> DimEmployeeEducations { get; set; }
    public virtual ICollection<FactSalaryDbModel> FactSalaries { get; set; }

    public DimEmployeeDbModel() 
    {
        DimEmployeeEducations = new List<DimEmployeeEducationDbModel>();
        FactSalaries = new List<FactSalaryDbModel>();
    }
}
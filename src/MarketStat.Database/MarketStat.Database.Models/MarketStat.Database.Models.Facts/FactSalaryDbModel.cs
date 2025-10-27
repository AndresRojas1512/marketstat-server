using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models.MarketStat.Database.Models.Facts;

[Table("fact_salaries")]
public class FactSalaryDbModel
{
    [Key]
    [Column("salary_fact_id")]
    public long SalaryFactId { get; set; }
    
    [Required]
    [Column("date_id")]
    public int DateId { get; set; }
    
    [Required]
    [Column("location_id")]
    public int LocationId { get; set; }
    
    [Required]
    [Column("employer_id")]
    public int EmployerId { get; set; }
    
    [Required]
    [Column("job_id")]
    public int JobId { get; set; }
    
    [Required]
    [Column("employee_id")]
    public int EmployeeId { get; set; }
    
    [Required]
    [Column("salary_amount", TypeName = "numeric(18, 2)")]
    public decimal SalaryAmount { get; set; }
    
    
    [ForeignKey(nameof(DateId))]
    public virtual DimDateDbModel? DimDate { get; set; }

    [ForeignKey(nameof(LocationId))]
    public virtual DimLocationDbModel? DimLocation { get; set; }

    [ForeignKey(nameof(EmployerId))]
    public virtual DimEmployerDbModel? DimEmployer { get; set; }

    [ForeignKey(nameof(JobId))]
    public virtual DimJobDbModel? DimJob { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public virtual DimEmployeeDbModel? DimEmployee { get; set; }
}
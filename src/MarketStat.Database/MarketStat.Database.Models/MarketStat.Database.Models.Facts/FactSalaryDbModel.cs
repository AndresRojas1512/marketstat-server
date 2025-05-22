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
    [Column("city_id")]
    public int CityId { get; set; }
    
    [Required]
    [Column("employer_id")]
    public int EmployerId { get; set; }
    
    [Required]
    [Column("job_role_id")]
    public int JobRoleId { get; set; }
    
    [Required]
    [Column("employee_id")]
    public int EmployeeId { get; set; }
    
    [Required]
    [Column("salary_amount", TypeName = "numeric(18, 2)")]
    public decimal SalaryAmount { get; set; }
    
    [Required]
    [Column("bonus_amount", TypeName = "numeric(18, 2)")]
    public decimal BonusAmount { get; set; }
    
    [ForeignKey(nameof(DateId))]
    public virtual DimDateDbModel? DimDate { get; set; }

    [ForeignKey(nameof(CityId))]
    public virtual DimCityDbModel? DimCity { get; set; }

    [ForeignKey(nameof(EmployerId))]
    public virtual DimEmployerDbModel? DimEmployer { get; set; }

    [ForeignKey(nameof(JobRoleId))]
    public virtual DimJobRoleDbModel? DimJobRole { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public virtual DimEmployeeDbModel? DimEmployee { get; set; }

    public FactSalaryDbModel(
        int dateId,
        int cityId,
        int employerId,
        int jobRoleId,
        int employeeId,
        decimal salaryAmount,
        decimal bonusAmount,
        long salaryFactId = 0
    )
    {
        SalaryFactId = salaryFactId;
        DateId = dateId;
        CityId = cityId;
        EmployerId = employerId;
        JobRoleId = jobRoleId;
        EmployeeId = employeeId;
        SalaryAmount = salaryAmount;
        BonusAmount = bonusAmount;
    }
}
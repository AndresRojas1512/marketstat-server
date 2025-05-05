using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models.MarketStat.Database.Models.Facts;

[Table("fact_salaries")]
public class FactSalaryDbModel
{
    [Key]
    [Column("salary_fact_id")]
    public int SalaryFactId { get; set; }
    
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
    [Column("salary_amount")]
    public decimal SalaryAmount { get; set; }
    
    [Required]
    [Column("bonus_amount")]
    public decimal BonusAmount { get; set; }

    public FactSalaryDbModel(int salaryFactId, int dateId, int cityId, int employerId, int jobRoleId, int employeeId,
        decimal salaryAmount, decimal bonusAmount)
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
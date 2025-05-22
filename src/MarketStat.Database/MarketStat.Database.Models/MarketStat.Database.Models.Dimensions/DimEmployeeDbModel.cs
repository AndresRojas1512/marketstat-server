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
    [Column("birth_date")]
    public DateOnly BirthDate { get; set; }
    
    [Required]
    [Column("career_start_date")]
    public DateOnly CareerStartDate { get; set; }
    
    public virtual ICollection<DimEmployeeEducationDbModel> DimEmployeeEducations { get; set; }
    public virtual ICollection<FactSalaryDbModel> FactSalaries { get; set; } = new List<FactSalaryDbModel>();
    
    public DimEmployeeDbModel() 
    {
        DimEmployeeEducations = new List<DimEmployeeEducationDbModel>();
        FactSalaries = new List<FactSalaryDbModel>();
    } 

    public DimEmployeeDbModel(int employeeId, DateOnly birthDate, DateOnly careerStartDate)
    {
        EmployeeId = employeeId;
        BirthDate = birthDate;
        CareerStartDate = careerStartDate;
        FactSalaries = new List<FactSalaryDbModel>();
    }
}
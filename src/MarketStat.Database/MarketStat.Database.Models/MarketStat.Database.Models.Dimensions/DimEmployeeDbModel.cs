using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_employees")]
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

    public DimEmployeeDbModel(int employeeId, DateOnly birthDate, DateOnly careerStartDate)
    {
        EmployeeId = employeeId;
        BirthDate = birthDate;
        CareerStartDate = careerStartDate;
    }
}
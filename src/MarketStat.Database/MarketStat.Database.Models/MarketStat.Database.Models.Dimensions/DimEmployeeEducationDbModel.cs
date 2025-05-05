using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_employee_education")]
public class DimEmployeeEducationDbModel
{
    [Key, Column("employee_id", Order = 0)]
    public int EmployeeId { get; set; }
    
    [Key, Column("education_id", Order = 1)]
    public int EducationId { get; set; }
    
    [Required]
    [Column("graduation_year")]
    public short GraduationYear { get; set; }

    public DimEmployeeEducationDbModel(int employeeId, int educationId, short graduationYear)
    {
        EmployeeId = employeeId;
        EducationId = educationId;
        GraduationYear = graduationYear;
    }
}
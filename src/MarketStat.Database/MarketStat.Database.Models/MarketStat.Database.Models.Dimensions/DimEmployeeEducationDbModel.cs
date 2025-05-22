using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_employee_education")]
public class DimEmployeeEducationDbModel
{
    [Column("employee_id")]
    public int EmployeeId { get; set; }
    
    [Column("education_id")]
    public int EducationId { get; set; }
    
    [Required]
    [Column("graduation_year")]
    public short GraduationYear { get; set; }
    
    [ForeignKey(nameof(EmployeeId))]
    public virtual DimEmployeeDbModel? Employee { get; set; }
    
    [ForeignKey(nameof(EducationId))]
    public virtual DimEducationDbModel? Education { get; set; }
    
    public DimEmployeeEducationDbModel() { }

    public DimEmployeeEducationDbModel(int employeeId, int educationId, short graduationYear)
    {
        EmployeeId = employeeId;
        EducationId = educationId;
        GraduationYear = graduationYear;
    }
}
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;

namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEmployee
{
    public int EmployeeId { get; set; }
    public string EmployeeRefId { get; set; }
    public DateOnly BirthDate { get; set; }
    public DateOnly CareerStartDate { get; set; }
    public string? Gender { get; set; }
    public int? EducationId { get; set; }
    public short? GraduationYear { get; set; }
    public virtual DimEducation? Education { get; set; }
    
    public virtual ICollection<FactSalary> FactSalaries { get; set; }
    
    public DimEmployee()
    {
        EmployeeRefId = string.Empty;
        FactSalaries = new List<FactSalary>();
    }
    
    public DimEmployee(int employeeId, string employeeRefId, DateOnly birthDate, DateOnly careerStartDate, string? gender, int? educationId, short? graduationYear)
    {
        EmployeeId = employeeId;
        EmployeeRefId = employeeRefId ?? throw new ArgumentNullException(nameof(employeeRefId));
        BirthDate = birthDate;
        CareerStartDate = careerStartDate;
        Gender = gender;
        EducationId = educationId;
        GraduationYear = graduationYear;
            
        FactSalaries = new List<FactSalary>();
    }
}
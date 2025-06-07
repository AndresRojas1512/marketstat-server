using MarketStat.Common.Core.MarketStat.Common.Core.Facts;

namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEmployee
{
    public int EmployeeId { get; set; }
    public string EmployeeRefId { get; set; }
    public DateOnly BirthDate { get; set; }
    public DateOnly CareerStartDate { get; set; }
    public string? Gender { get; set; }

    public virtual ICollection<DimEmployeeEducation> DimEmployeeEducations { get; set; }
    public virtual ICollection<FactSalary> FactSalaries { get; set; }
    
    public DimEmployee()
    {
        EmployeeRefId = string.Empty; 
        
        DimEmployeeEducations = new List<DimEmployeeEducation>();
        FactSalaries = new List<FactSalary>();
    }
    
    public DimEmployee(
        int employeeId, 
        string employeeRefId, 
        DateOnly birthDate, 
        DateOnly careerStartDate, 
        string? gender)
    {
        EmployeeId = employeeId;
        EmployeeRefId = employeeRefId ?? throw new ArgumentNullException(nameof(employeeRefId));
        BirthDate = birthDate;
        CareerStartDate = careerStartDate;
        Gender = gender;
            
        DimEmployeeEducations = new List<DimEmployeeEducation>();
        FactSalaries = new List<FactSalary>();
    }
}
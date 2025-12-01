namespace MarketStat.Common.Core.Dimensions;

using MarketStat.Common.Core.Facts;

public class DimEmployee
{
    public DimEmployee()
    {
        EmployeeRefId = string.Empty;
    }

    public DimEmployee(
        int employeeId,
        string employeeRefId,
        DateOnly birthDate,
        DateOnly careerStartDate,
        string? gender,
        int? educationId,
        short? graduationYear)
    {
        EmployeeId = employeeId;
        EmployeeRefId = employeeRefId ?? throw new ArgumentNullException(nameof(employeeRefId));
        BirthDate = birthDate;
        CareerStartDate = careerStartDate;
        Gender = gender;
        EducationId = educationId;
        GraduationYear = graduationYear;
    }

    public int EmployeeId { get; set; }

    public string EmployeeRefId { get; set; }

    public DateOnly BirthDate { get; set; }

    public DateOnly CareerStartDate { get; set; }

    public string? Gender { get; set; }

    public int? EducationId { get; set; }

    public short? GraduationYear { get; set; }

    public virtual DimEducation? Education { get; set; }

    public virtual ICollection<FactSalary> FactSalaries { get; } = new List<FactSalary>();
}

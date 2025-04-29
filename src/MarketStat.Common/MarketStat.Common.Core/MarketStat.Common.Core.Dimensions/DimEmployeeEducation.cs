namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEmployeeEducation
{
    public int EmployeeId { get; set; }
    public int EducationId { get; set; }
    public short GraduationYear { get; set; }

    public DimEmployeeEducation(int employeeId, int educationId, short graduationYear)
    {
        EmployeeId = employeeId;
        EducationId = educationId;
        GraduationYear = graduationYear;
    }
}
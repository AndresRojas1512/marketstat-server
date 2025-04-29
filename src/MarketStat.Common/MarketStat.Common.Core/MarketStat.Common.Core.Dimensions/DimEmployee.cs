namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEmployee
{
    public int EmployeeId { get; set; }
    public DateOnly BirthDate { get; set; }
    
    public DateOnly CareerStartDate { get; set; }

    public DimEmployee(int employeeId, DateOnly birthDate, DateOnly careerStartDate)
    {
        EmployeeId = employeeId;
        BirthDate = birthDate;
        CareerStartDate = careerStartDate;
    }
}
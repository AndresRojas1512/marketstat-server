namespace MarketStat.Services.Dimensions.DimEmployeeService.Validators;

public class DimEmployeeValidator
{
    public static void ValidateParameters(int employeeId, DateOnly birthDate, DateOnly careerStartDate)
    {
        if (employeeId <= 0)
            throw new ArgumentException("EmployeeId must be a positive integer.");
        
        if (birthDate == default)
            throw new ArgumentException("BirthDate must be provided.");
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("BirthDate cannot be in the future.");
        
        if (careerStartDate == default)
            throw new ArgumentException("CareerStartDate must be provided.");
        if (careerStartDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("CareerStartDate cannot be in the future.");
    }
}
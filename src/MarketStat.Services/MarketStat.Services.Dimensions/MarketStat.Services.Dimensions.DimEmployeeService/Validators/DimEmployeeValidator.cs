namespace MarketStat.Services.Dimensions.DimEmployeeService.Validators;

public class DimEmployeeValidator
{
    public static void ValidateParameters(int employeeId, DateOnly birthDate, bool checkId = true)
    {
        if (checkId && employeeId <= 0)
            throw new ArgumentException("EmployeeId must be a positive integer.");
        if (birthDate == default)
            throw new ArgumentException("BirthDate must be provided.");
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("BirthDate cannot be in the future.");
    }
}
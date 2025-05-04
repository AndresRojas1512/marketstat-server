namespace MarketStat.Services.Dimensions.DimEmployeeService.Validators;

public static class DimEmployeeValidator
{
    public static void ValidateForCreate(DateOnly birthDate, DateOnly careerStartDate)
    {
        if (birthDate == default)
            throw new ArgumentException("BirthDate must be provided.");
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("BirthDate cannot be in the future.");
        
        if (careerStartDate == default)
            throw new ArgumentException("CareerStartDate must be provided.");
        if (careerStartDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("CareerStartDate cannot be in the future.");
        if (careerStartDate < birthDate)
            throw new ArgumentException("Career start date cannot be earlier than birth date.", nameof(careerStartDate));
    }
    public static void ValidateForUpdate(int employeeId, DateOnly birthDate, DateOnly careerStartDate)
    {
        if (employeeId <= 0)
            throw new ArgumentException("EmployeeId must be a positive integer.");
        ValidateForCreate(birthDate, careerStartDate);
    }
}
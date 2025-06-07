namespace MarketStat.Services.Dimensions.DimEmployeeService.Validators;

public static class DimEmployeeValidator
{
    private const int MaxRefIdLength = 255;
    private const int MaxGenderLength = 50;
    private const int MinCareerAgeYears = 16;

    public static void ValidateForCreate(
        string employeeRefId, 
        DateOnly birthDate, 
        DateOnly careerStartDate, 
        string? gender)
    {
        if (string.IsNullOrWhiteSpace(employeeRefId))
            throw new ArgumentException("EmployeeRefId is required.", nameof(employeeRefId));
        if (employeeRefId.Length > MaxRefIdLength)
            throw new ArgumentException($"EmployeeRefId must be {MaxRefIdLength} characters or fewer.", nameof(employeeRefId));

        if (birthDate == default)
            throw new ArgumentException("BirthDate must be provided.", nameof(birthDate));
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("BirthDate cannot be in the future.", nameof(birthDate));
        
        if (careerStartDate == default)
            throw new ArgumentException("CareerStartDate must be provided.", nameof(careerStartDate));
        if (careerStartDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("CareerStartDate cannot be in the future.", nameof(careerStartDate));
        
        if (careerStartDate < birthDate)
            throw new ArgumentException("Career start date cannot be earlier than birth date.", nameof(careerStartDate));
        if (careerStartDate < birthDate.AddYears(MinCareerAgeYears))
            throw new ArgumentException($"Career start date must be at least {MinCareerAgeYears} years after birth date.", nameof(careerStartDate));

        if (gender != null && gender.Length > MaxGenderLength)
            throw new ArgumentException($"Gender must be {MaxGenderLength} characters or fewer.", nameof(gender));
    }

    public static void ValidateForUpdate(
        int employeeId, 
        string employeeRefId, 
        DateOnly birthDate, 
        DateOnly careerStartDate, 
        string? gender)
    {
        if (employeeId <= 0)
            throw new ArgumentException("EmployeeId must be a positive integer for update.", nameof(employeeId));
        
        ValidateForCreate(employeeRefId, birthDate, careerStartDate, gender);
    }
}
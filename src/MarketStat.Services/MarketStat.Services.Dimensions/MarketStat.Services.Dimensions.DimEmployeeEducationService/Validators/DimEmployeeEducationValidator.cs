namespace MarketStat.Services.Dimensions.DimEmployeeEducationService.Validators;

public class DimEmployeeEducationValidator
{
    public static void ValidateParameters(int employeeId, int educationId, short graduationYear)
    {
        if (employeeId <= 0)
            throw new ArgumentException("EmployeeId must be a positive integer.");
        if (educationId <= 0)
            throw new ArgumentException("EducationId must be a positive integer.");
        if (graduationYear < 1900 || graduationYear > DateTime.Now.Year + 10)
            throw new ArgumentException("GraduationYear is our of range.");
    }
}
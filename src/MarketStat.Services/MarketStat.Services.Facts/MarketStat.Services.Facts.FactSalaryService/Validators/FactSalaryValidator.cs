namespace MarketStat.Services.Facts.FactSalaryService.Validators;

public class FactSalaryValidator
{
    public static void ValidateParameters(int salaryFactId, int dateId, int cityId, int employerId, int jobRoleId,
        int employeeId, decimal salaryAmount, decimal bonusAmount)
    {
        if (salaryFactId <= 0)
            throw new ArgumentException("SalaryFactId must be a positive integer.");

        if (dateId <= 0)
            throw new ArgumentException("DateId must be a positive integer.");
        if (cityId <= 0)
            throw new ArgumentException("CityId must be a positive integer.");
        if (employerId <= 0)
            throw new ArgumentException("EmployerId must be a positive integer.");
        if (jobRoleId <= 0)
            throw new ArgumentException("JobRoleId must be a positive integer.");
        if (employeeId <= 0)
            throw new ArgumentException("EmployeeId must be a positive integer.");

        if (salaryAmount < 0)
            throw new ArgumentException("SalaryAmount cannot be negative.");
        if (bonusAmount < 0)
            throw new ArgumentException("BonusAmount cannot be negative.");
    }
}
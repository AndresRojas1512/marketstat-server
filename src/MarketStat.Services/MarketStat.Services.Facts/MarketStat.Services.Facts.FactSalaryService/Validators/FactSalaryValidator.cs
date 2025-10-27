namespace MarketStat.Services.Facts.FactSalaryService.Validators;

public static class FactSalaryValidator
{
    public static void ValidateForCreate(int dateId, int locationId, int employerId, int jobId, int employeeId,
        decimal salaryAmount)
    {
        if (dateId <= 0)
            throw new ArgumentException("DateId must be a positive integer.");
        if (locationId <= 0)
            throw new ArgumentException("LocationId must be a positive integer.");
        if (employerId <= 0)
            throw new ArgumentException("EmployerId must be a positive integer.");
        if (jobId <= 0)
            throw new ArgumentException("JobId must be a positive integer.");
        if (employeeId <= 0)
            throw new ArgumentException("EmployeeId must be a positive integer.");

        if (salaryAmount < 0)
            throw new ArgumentException("SalaryAmount cannot be negative.");
    }
    
    public static void ValidateForUpdate(long salaryFactId, int dateId, int locationId, int employerId, int jobId,
        int employeeId, decimal salaryAmount)
    {
        if (salaryFactId <= 0)
            throw new ArgumentException("SalaryFactId must be a positive integer.");
        ValidateForCreate(dateId, locationId, employerId, jobId, employeeId, salaryAmount);
    }
}
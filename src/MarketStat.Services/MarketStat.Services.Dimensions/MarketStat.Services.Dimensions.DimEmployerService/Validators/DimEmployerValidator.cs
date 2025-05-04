namespace MarketStat.Services.Dimensions.DimEmployerService.Validators;

public class DimEmployerValidator
{
    public static void ValidateForCreate(string employerName, bool isPublic)
    {
        if (string.IsNullOrWhiteSpace(employerName))
            throw new ArgumentException("Employer name is required");
        if (employerName.Length > 255)
            throw new ArgumentException("Employer name must be 255 less characters or fewer");
    }

    public static void ValidateForUpdate(int employerId, string employerName, bool isPublic)
    {
        if (employerId <= 0)
            throw new ArgumentException("EmployerId must be a positive integer.");
        ValidateForCreate(employerName, isPublic);
    }
}
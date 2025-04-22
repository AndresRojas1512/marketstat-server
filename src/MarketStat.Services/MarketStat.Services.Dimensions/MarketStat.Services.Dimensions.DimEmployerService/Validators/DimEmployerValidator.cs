namespace MarketStat.Services.Dimensions.DimEmployerService.Validators;

public class DimEmployerValidator
{
    public static void ValidateParameters(int employerId, string employerName, string industry, bool isPublic)
    {
        if (employerId <= 0)
            throw new ArgumentException("EmployerId must be a positive integer.");
        if (string.IsNullOrWhiteSpace(employerName))
            throw new ArgumentException("Employer name is required");
        if (employerName.Length > 255)
            throw new ArgumentException("Employer name must be 255 less characters or fewer");
        if (!string.IsNullOrEmpty(industry) && industry.Length > 255)
            throw new ArgumentException("Industry must be 100 characters or fewer");
    }
}
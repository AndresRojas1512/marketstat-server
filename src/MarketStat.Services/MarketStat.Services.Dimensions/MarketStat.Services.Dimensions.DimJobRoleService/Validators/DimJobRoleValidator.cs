namespace MarketStat.Services.Dimensions.DimJobRoleService.Validators;

public class DimJobRoleValidator
{
    public static void ValidateParameters(int jobRoleId, string jobRoleTitle, string seniorityLevel, int industryFieldId, bool checkId = true)
    {
        if (checkId && jobRoleId <= 0)
            throw new ArgumentException("JobRoleId must be a positive integer.");

        if (string.IsNullOrWhiteSpace(jobRoleTitle))
            throw new ArgumentException("Job role title is required.");
        if (jobRoleTitle.Length > 255)
            throw new ArgumentException("Job role title must be 255 characters or fewer.");

        if (!string.IsNullOrEmpty(seniorityLevel) && seniorityLevel.Length > 50)
            throw new ArgumentException("Seniority level must be 50 characters or fewer.");

        if (industryFieldId <= 0)
            throw new ArgumentException("IndustryFieldId must be a positive integer.");
    }
}
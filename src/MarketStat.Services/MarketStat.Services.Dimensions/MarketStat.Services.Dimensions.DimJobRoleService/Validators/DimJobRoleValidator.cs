namespace MarketStat.Services.Dimensions.DimJobRoleService.Validators;

public class DimJobRoleValidator
{
    public static void ValidateParameters(int jobRoleId, string jobRoleTitle, int standardJobRoleId, int hierarchyLevelId)
    {
        if (jobRoleId <= 0)
            throw new ArgumentException("JobRoleId must be a positive integer.");

        if (string.IsNullOrWhiteSpace(jobRoleTitle))
            throw new ArgumentException("Job role title is required.");
        if (jobRoleTitle.Length > 255)
            throw new ArgumentException("Job role title must be 255 characters or fewer.");

        if (standardJobRoleId <= 0)
            throw new ArgumentException("IndustryFieldId must be a positive integer.");
        
        if (hierarchyLevelId <= 0)
            throw new ArgumentException("hierarchyLevelId must be a positive integer.");
    }
}
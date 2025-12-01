namespace MarketStat.Services.Dimensions.DimJobService.Validators;

public static class DimJobValidator
{
    public static void ValidateForCreate(
        string jobRoleTitle,
        string standardJobRoleTitle,
        string hierarchyLevelName,
        int industryFieldId)
    {
        if (string.IsNullOrWhiteSpace(jobRoleTitle))
        {
            throw new ArgumentException("JobRoleTitle is required.", nameof(jobRoleTitle));
        }

        if (string.IsNullOrWhiteSpace(standardJobRoleTitle))
        {
            throw new ArgumentException("StandardJobRoleTitle is required.", nameof(standardJobRoleTitle));
        }

        if (string.IsNullOrWhiteSpace(hierarchyLevelName))
        {
            throw new ArgumentException("HierarchyLevelName is required.", nameof(hierarchyLevelName));
        }

        if (industryFieldId <= 0)
        {
            throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(industryFieldId));
        }
    }

    public static void ValidateForUpdate(
        int jobId,
        string jobRoleTitle,
        string standardJobRoleTitle,
        string hierarchyLevelName,
        int industryFieldId)
    {
        if (jobId <= 0)
        {
            throw new ArgumentException("JobId must be a positive integer.", nameof(jobId));
        }

        ValidateForCreate(jobRoleTitle, standardJobRoleTitle, hierarchyLevelName, industryFieldId);
    }
}

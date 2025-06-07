namespace MarketStat.Services.Dimensions.DimStandardJobRoleService.Validators;

public static class DimStandardJobRoleValidator
{
    private const int MaxCodeLength = 20;
    private const int MaxNameLength = 255;

    public static void ValidateForCreate(string jobRoleCode, string jobRoleName, int industryFieldId)
    {
        if (string.IsNullOrWhiteSpace(jobRoleCode))
            throw new ArgumentException("Job role code is required.", nameof(jobRoleCode));
        if (jobRoleCode.Length > MaxCodeLength)
            throw new ArgumentException($"Job role code must be {MaxCodeLength} characters or fewer.", nameof(jobRoleCode));

        if (string.IsNullOrWhiteSpace(jobRoleName))
            throw new ArgumentException("Job role name is required.", nameof(jobRoleName));
        if (jobRoleName.Length > MaxNameLength)
            throw new ArgumentException($"Job role name must be {MaxNameLength} characters or fewer.", nameof(jobRoleName));
            
        if (industryFieldId <= 0)
            throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(industryFieldId));
    }
    
    public static void ValidateForUpdate(int id, string jobRoleCode, string jobRoleName, int industryFieldId)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be a positive integer.", nameof(id));

        ValidateForCreate(jobRoleCode, jobRoleName, industryFieldId);
    }
}
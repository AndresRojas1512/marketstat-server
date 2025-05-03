namespace MarketStat.Services.Dimensions.DimStandardJobRoleService.Validators;

public static class DimStandardJobRoleValidator
{
    public static void ValidateParameters(int id, string jobRoleName, int industryFieldId)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero");

        if (string.IsNullOrWhiteSpace(jobRoleName) || jobRoleName.Length > 255)
            throw new ArgumentException("Job role name must be between 0 and 255 characters");
        
        if (industryFieldId <= 0)
            throw new ArgumentException("Industry field Id must be greater than zero");
    }
}
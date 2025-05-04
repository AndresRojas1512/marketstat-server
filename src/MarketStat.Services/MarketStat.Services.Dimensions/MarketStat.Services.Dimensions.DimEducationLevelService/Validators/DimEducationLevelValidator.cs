namespace MarketStat.Services.Dimensions.DimEducationLevelService.Validators;

public static class DimEducationLevelValidator
{
    public static void ValidateForCreate(string educationLevelName)
    {
        if (string.IsNullOrWhiteSpace(educationLevelName) || educationLevelName.Length > 255)
            throw new ArgumentException("Invalid education level name.");
    }

    public static void ValidateForUpdate(int educationLevelId, string educationLevelName)
    {
        if (educationLevelId <= 0)
            throw new ArgumentException("EducationLevelId must be positive", nameof(educationLevelId));
        ValidateForCreate(educationLevelName);
    }
}
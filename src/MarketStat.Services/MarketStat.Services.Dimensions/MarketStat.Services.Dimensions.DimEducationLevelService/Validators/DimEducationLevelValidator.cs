namespace MarketStat.Services.Dimensions.DimEducationLevelService.Validators;

public static class DimEducationLevelValidator
{
    public static void ValidateParameters(int educationLevelId, string educationLevelName)
    {
        if (educationLevelId <= 0)
            throw new ArgumentException("EducationLevelId must be positive", nameof(educationLevelId));

        if (string.IsNullOrWhiteSpace(educationLevelName) || educationLevelName.Length > 255)
            throw new ArgumentException("Invalid education level name.");
    }
}
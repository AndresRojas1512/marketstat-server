namespace MarketStat.Common.Validators.Dimensions;

public class DimEducationValidator
{
    public static void ValidateForCreate(string specialty, string specialtyCode, string educationLevelName)
    {
        
        if (string.IsNullOrWhiteSpace(specialty))
            throw new ArgumentException("Specialty is required.");
        if (specialty.Length > 255)
            throw new ArgumentException("Specialty cannot exceed 255 characters.");
        
        if (string.IsNullOrWhiteSpace(specialtyCode))
            throw new ArgumentException("SpecialtyCode is required.");
        if (specialtyCode.Length > 255)
            throw new ArgumentException("SpecialtyCode cannot exceed 255 characters.");

        if (string.IsNullOrWhiteSpace(educationLevelName))
            throw new ArgumentException("EducationLevelName is required.");
        if (educationLevelName.Length > 255)
            throw new ArgumentException("EducationLevelName cannot exceed 255 characters.");
    }

    public static void ValidateForUpdate(int educationId, string specialty, string specialtyCode, string educationLevelName)
    {
        if (educationId <= 0)
            throw new ArgumentException("EducationId must be a positive integer.");
        ValidateForCreate(specialty, specialtyCode, educationLevelName);
    }
}
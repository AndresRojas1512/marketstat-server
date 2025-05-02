namespace MarketStat.Services.Dimensions.DimEducationService.Validators;

public class DimEducationValidator
{
    public static void ValidateParameters(int educationId, string specialty, string specialtyCode, int educationLevelId, int industryFieldId)
    {
        if (educationId <= 0)
            throw new ArgumentException("EducationId must be a positive integer.");
        
        if (string.IsNullOrWhiteSpace(specialty))
            throw new ArgumentException("Specialty is required.");
        if (specialty.Length > 255)
            throw new ArgumentException("Specialty cannot exceed 255 characters.");
        
        if (string.IsNullOrWhiteSpace(specialtyCode))
            throw new ArgumentException("SpecialtyCode is required.");
        if (specialtyCode.Length > 255)
            throw new ArgumentException("SpecialtyCode cannot exceed 255 characters.");

        if (educationLevelId <= 0)
            throw new ArgumentException("EducationLevelId must be a positiver integer.");
        
        if (industryFieldId <= 0)
            throw new ArgumentException("IndustryFieldId must be a positive integer.");
        
    }
}
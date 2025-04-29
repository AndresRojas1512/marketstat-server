using MarketStat.Common.Enums;

namespace MarketStat.Services.Dimensions.DimEducationService.Validators;

public class DimEducationValidator
{
    public static void ValidateParameters(int educationId, string specialization, EducationLevel educationLevel, int industryFieldId)
    {
        if (educationId <= 0)
            throw new ArgumentException("EducationId must be a positive integer.");
        
        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("Specialization is required.");
        if (specialization.Length > 255)
            throw new ArgumentException("Specialization cannot exceed 255 characters.");
        
        if (!Enum.IsDefined(typeof(EducationLevel), educationLevel))
            throw new ArgumentException($"Invalid education level: {educationLevel}", nameof(educationLevel));
        
        if (industryFieldId <= 0)
            throw new ArgumentException("IndustryFieldId must be a positive integer.");
        
    }
}
namespace MarketStat.Services.Dimensions.DimEducationService.Validators;

public class DimEducationValidator
{
    public static void ValidateParameters(int educationId, string specialization, string educationLevel)
    {
        if (educationId <= 0)
            throw new ArgumentException("EducationId must be a positive integer.");
        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("Specialization is required.");
        if (specialization.Length > 255)
            throw new ArgumentException("Specialization cannot exceed 255 characters.");
        if (string.IsNullOrWhiteSpace(educationLevel))
            throw new ArgumentException("Education level is required.");
        if (educationLevel.Length > 50)
            throw new ArgumentException("Education level cannot exceed 50 characters.");
    }
}
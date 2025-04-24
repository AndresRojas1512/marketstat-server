namespace MarketStat.Services.Dimensions.DimIndustryFieldService.Validators;

public class DimIndustryFieldValidator
{
    public static void ValidateParameters(int industryFieldId, string industryFieldName, bool checkId = true)
    {
        if (checkId && industryFieldId <= 0)
            throw new ArgumentException("IndustryFieldId must be a positive integer.");
        if (string.IsNullOrWhiteSpace(industryFieldName))
            throw new ArgumentException("Industry field name is required.");
        if (industryFieldName.Length > 255)
            throw new ArgumentException("Industry field name must be 255 characters or fewer.");
    }
}
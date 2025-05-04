namespace MarketStat.Services.Dimensions.DimIndustryFieldService.Validators;

public static class DimIndustryFieldValidator
{
    public static void ValidateForCreate(string industryFieldName)
    {
        if (string.IsNullOrWhiteSpace(industryFieldName))
            throw new ArgumentException("Industry field name is required.");
        if (industryFieldName.Length > 255)
            throw new ArgumentException("Industry field name must be 255 characters or fewer.");
    }
    public static void ValidateForUpdate(int industryFieldId, string industryFieldName)
    {
        if (industryFieldId <= 0)
            throw new ArgumentException("IndustryFieldId must be a positive integer.");
        ValidateForCreate(industryFieldName);
    }
}
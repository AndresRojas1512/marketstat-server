namespace MarketStat.Services.Dimensions.DimIndustryFieldService.Validators;

public static class DimIndustryFieldValidator
{
    private const int MaxCodeLength = 10;
    private const int MaxNameLength = 255;

    public static void ValidateForCreate(string industryFieldCode, string industryFieldName)
    {
        if (string.IsNullOrWhiteSpace(industryFieldCode))
        {
            throw new ArgumentException("Industry field code is required.", nameof(industryFieldCode));
        }

        if (industryFieldCode.Length > MaxCodeLength)
        {
            throw new ArgumentException($"Industry field code must be {MaxCodeLength} characters or fewer.", nameof(industryFieldCode));
        }

        if (string.IsNullOrWhiteSpace(industryFieldName))
        {
            throw new ArgumentException("Industry field name is required.", nameof(industryFieldName));
        }

        if (industryFieldName.Length > MaxNameLength)
        {
            throw new ArgumentException($"Industry field name must be {MaxNameLength} characters or fewer.", nameof(industryFieldName));
        }
    }

    public static void ValidateForUpdate(int industryFieldId, string industryFieldCode, string industryFieldName)
    {
        if (industryFieldId <= 0)
        {
            throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(industryFieldId));
        }

        ValidateForCreate(industryFieldCode, industryFieldName);
    }
}

namespace MarketStat.Services.Dimensions.DimEmployerIndustryFieldService.Validators;

public class DimEmployerIndustryFieldValidator
{
    public static void ValidateParameters(int employerId, int industryFieldId)
    {
        if (employerId <= 0)
            throw new ArgumentException($"Invalid Employer ID: {employerId}.");
        if (industryFieldId <= 0)
            throw new ArgumentException($"Invalid Industry Field ID: {industryFieldId}.");
    }
}
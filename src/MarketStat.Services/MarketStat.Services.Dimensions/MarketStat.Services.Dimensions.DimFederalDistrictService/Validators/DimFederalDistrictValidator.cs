namespace MarketStat.Services.Dimensions.DimFederalDistrictService.Validators;

public class DimFederalDistrictValidator
{
    public static void ValidateParameters(int districtId, string districtName)
    {
        if (districtId <= 0)
            throw new ArgumentException("DistrictId must be greater than 0");
        
        if (string.IsNullOrWhiteSpace(districtName) ||districtName.Length > 255)
            throw new ArgumentException("DistrictName must be less than 255 characters");
    }
}
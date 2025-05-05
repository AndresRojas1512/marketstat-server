namespace MarketStat.Services.Dimensions.DimFederalDistrictService.Validators;

public static class DimFederalDistrictValidator
{
    public static void ValidateForCreate(string districtName)
    {
        if (string.IsNullOrWhiteSpace(districtName) ||districtName.Length > 255)
            throw new ArgumentException("DistrictName must be less than 255 characters");
    }
    
    public static void ValidateForUpdate(int districtId, string districtName)
    {
        if (districtId <= 0)
            throw new ArgumentException("DistrictId must be greater than 0");
        ValidateForCreate(districtName);
    }
}
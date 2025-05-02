namespace MarketStat.Services.Dimensions.DimOblastService.Validators;

public class DimOblastValidator
{
    public static void ValidateParameters(int oblastId, string oblastName, int districtId)
    {
        if (oblastId <= 0)
            throw new ArgumentException("Oblast id must be greater than zero.");
        
        if (string.IsNullOrWhiteSpace(oblastName) || oblastName.Length > 255)
            throw new ArgumentException("Oblast name must be less than 255 characters.");
        
        if (districtId <= 0)
            throw new ArgumentException("District id must be greater than zero.");
    }
}
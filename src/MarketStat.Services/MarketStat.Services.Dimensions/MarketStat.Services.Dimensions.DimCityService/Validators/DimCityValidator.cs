namespace MarketStat.Services.Dimensions.DimCityService.Validators;

public class DimCityValidator
{
    public static void ValidateParameters(int cityId, string cityName, int oblastId)
    {
        if (cityId <= 0)
            throw new ArgumentException("CityId must be a positive integer.");

        if (string.IsNullOrWhiteSpace(cityName))
            throw new ArgumentException("City name is required.");
        if (cityName.Length > 255)
            throw new ArgumentException("City name must be 255 characters or fewer.");

        if (oblastId <= 0)
            throw new ArgumentException("OblastId must be a positive integer.");
    }
}
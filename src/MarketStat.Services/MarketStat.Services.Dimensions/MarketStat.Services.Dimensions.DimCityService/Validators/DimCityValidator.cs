namespace MarketStat.Services.Dimensions.DimCityService.Validators;

public class DimCityValidator
{
    public static void ValidateParameters(int cityId, string cityName, string oblastName, string federalDistrict)
    {
        if (cityId <= 0)
            throw new ArgumentException("CityId must be a positive integer.");

        if (string.IsNullOrWhiteSpace(cityName))
            throw new ArgumentException("City name is required.");
        if (cityName.Length > 255)
            throw new ArgumentException("City name must be 255 characters or fewer.");

        if (string.IsNullOrWhiteSpace(oblastName))
            throw new ArgumentException("Oblast name is required.");
        if (oblastName.Length > 255)
            throw new ArgumentException("Oblast name must be 255 characters or fewer.");

        if (string.IsNullOrWhiteSpace(federalDistrict))
            throw new ArgumentException("Federal district is required.");
        if (federalDistrict.Length > 255)
            throw new ArgumentException("Federal district must be 255 characters or fewer.");
    }
}
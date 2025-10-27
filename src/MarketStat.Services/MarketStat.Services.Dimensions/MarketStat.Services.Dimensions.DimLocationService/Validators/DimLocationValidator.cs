namespace MarketStat.Services.Dimencions.DimLocationService.Validators;

public class DimLocationValidator
{
    public static void ValidateForCreate(string cityName, string oblastName, string districtName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            throw new ArgumentException("City name is required.", nameof(cityName));
        if (string.IsNullOrWhiteSpace(oblastName))
            throw new ArgumentException("Oblast name is required.", nameof(oblastName));
        if (string.IsNullOrWhiteSpace(districtName))
            throw new ArgumentException("District name is required.", nameof(districtName));
    }

    public static void ValidateForUpdate(int locationId, string cityName, string oblastName, string districtName)
    {
        if (locationId <= 0)
            throw new ArgumentException("LocationId must be a positive integer.", nameof(locationId));
        ValidateForCreate(cityName, oblastName, districtName);
    }
}
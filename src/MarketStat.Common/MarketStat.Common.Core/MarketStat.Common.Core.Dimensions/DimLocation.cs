namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimLocation
{
    public int LocationId { get; set; }
    public string CityName { get; set; }
    public string OblastName { get; set; }
    public string DistrictName { get; set; }

    public DimLocation()
    {
    }

    public DimLocation(int locationId, string cityName, string oblastName, string districtName)
    {
        LocationId = locationId;
        CityName = cityName;
        OblastName = oblastName;
        DistrictName = districtName;
    }
}
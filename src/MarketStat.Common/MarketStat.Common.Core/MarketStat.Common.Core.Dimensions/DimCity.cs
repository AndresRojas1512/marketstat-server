namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimCity
{
    public int CityId { get; set; }
    public string CityName { get; set; }
    public int OblastId { get; set; }

    public DimCity(int cityId, string cityName, int oblastId)
    {
        CityId = cityId;
        CityName = cityName;
        OblastId = oblastId;
    }
}
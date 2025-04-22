namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimCity
{
    public int CityId { get; set; }
    public string CityName { get; set; }
    public string OblastName { get; set; }
    public string FederalDistrict { get; set; }

    public DimCity(int cityId, string cityName, string oblastName, string federalDistrict)
    {
        CityId = cityId;
        CityName = cityName;
        OblastName = oblastName;
        FederalDistrict = federalDistrict;
    }
}
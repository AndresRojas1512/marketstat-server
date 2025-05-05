namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimFederalDistrict
{
    public int DistrictId { get; set; }
    public string DistrictName { get; set; }

    public DimFederalDistrict(int districtId, string districtName)
    {
        DistrictId = districtId;
        DistrictName = districtName;
    }
}
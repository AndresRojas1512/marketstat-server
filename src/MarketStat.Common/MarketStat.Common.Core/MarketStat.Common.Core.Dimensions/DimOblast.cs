namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimOblast
{
    public int OblastId { get; set; }
    public string OblastName { get; set; }
    public int DistrictId { get; set; }

    public DimOblast()
    {
    }

    public DimOblast(int oblastId, string oblastName, int districtId)
    {
        OblastId = oblastId;
        OblastName = oblastName;
        DistrictId = districtId;
    }
}
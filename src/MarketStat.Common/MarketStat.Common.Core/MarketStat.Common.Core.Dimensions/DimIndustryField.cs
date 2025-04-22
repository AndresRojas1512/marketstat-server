namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimIndustryField
{
    public int IndustryFieldId { get; set; }
    public string IndustryFieldName { get; set; }

    public DimIndustryField(int industryFieldId, string industryFieldName)
    {
        IndustryFieldId = industryFieldId;
        IndustryFieldName = industryFieldName;
    }
}
namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimIndustryField
{
    public int IndustryFieldId { get; set; }
        
    public string IndustryFieldCode { get; set; }

    public string IndustryFieldName { get; set; }
    
    public DimIndustryField()
    {
        IndustryFieldCode = string.Empty;
        IndustryFieldName = string.Empty;
    }
    
    public DimIndustryField(int industryFieldId, string industryFieldCode, string industryFieldName)
    {
        IndustryFieldId = industryFieldId;
        IndustryFieldCode = industryFieldCode ?? throw new ArgumentNullException(nameof(industryFieldCode));
        IndustryFieldName = industryFieldName ?? throw new ArgumentNullException(nameof(industryFieldName));
    }
}
namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEmployerIndustryField
{
    public int EmployerId { get; set; }
    public int IndustryFieldId { get; set; }

    public DimEmployerIndustryField(int employerId, int industryFieldId)
    {
        EmployerId = employerId;
        IndustryFieldId = industryFieldId;
    }
}
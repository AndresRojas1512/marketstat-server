namespace MarketStat.Contracts.Dimensions.DimIndustryField;

public interface IGetDimIndustryFieldResponse
{
    int IndustryFieldId { get; }
    string IndustryFieldCode { get; }
    string IndustryFieldName { get; }
}
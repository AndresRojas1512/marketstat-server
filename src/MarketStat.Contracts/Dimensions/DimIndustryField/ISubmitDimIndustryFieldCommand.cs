namespace MarketStat.Contracts.Dimensions.DimIndustryField;

public interface ISubmitDimIndustryFieldCommand
{
    string IndustryFieldCode { get; }
    string IndustryFieldName { get; }
}
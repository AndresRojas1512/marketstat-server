namespace MarketStat.Contracts.Dimensions.DimIndustryField;

public interface ISubmitDimIndustryFieldUpdateCommand : ISubmitDimIndustryFieldCommand
{
    int IndustryFieldId { get; }
}
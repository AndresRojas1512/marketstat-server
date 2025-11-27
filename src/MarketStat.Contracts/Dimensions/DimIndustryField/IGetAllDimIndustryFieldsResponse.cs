namespace MarketStat.Contracts.Dimensions.DimIndustryField;

public interface IGetAllDimIndustryFieldsResponse
{
    List<IGetDimIndustryFieldResponse> IndustryFields { get; }
}
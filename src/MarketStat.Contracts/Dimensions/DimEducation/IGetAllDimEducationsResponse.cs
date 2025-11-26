namespace MarketStat.Contracts.Dimensions.DimEducation;

public interface IGetAllDimEducationsResponse
{
    List<IGetDimEducationResponse> Educations { get; }
}
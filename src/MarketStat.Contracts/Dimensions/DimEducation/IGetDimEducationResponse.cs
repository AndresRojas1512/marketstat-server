namespace MarketStat.Contracts.Dimensions.DimEducation;

public interface IGetDimEducationResponse
{
    int EducationId { get; }
    string SpecialtyName { get; }
    string SpecialtyCode { get; }
    string EducationLevelName { get; }
}
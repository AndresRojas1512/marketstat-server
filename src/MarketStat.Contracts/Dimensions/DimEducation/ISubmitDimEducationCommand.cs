namespace MarketStat.Contracts.Dimensions.DimEducation;

public interface ISubmitDimEducationCommand
{
    string SpecialtyName { get; }
    string SpecialtyCode { get; }
    string EducationLevelName { get; }
}
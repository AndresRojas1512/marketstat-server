namespace MarketStat.Contracts.Dimensions.DimEducation;

public interface ISubmitDimEducationUpdateCommand : ISubmitDimEducationCommand
{
    int EducationId { get; }
}
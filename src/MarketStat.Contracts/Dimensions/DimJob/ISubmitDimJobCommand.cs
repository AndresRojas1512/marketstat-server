namespace MarketStat.Contracts.Dimensions.DimJob;

public interface ISubmitDimJobCommand
{
    string JobRoleTitle { get; }
    string StandardJobRoleTitle { get; }
    string HierarchyLevelName { get; }
    int IndustryFieldId { get; }
}
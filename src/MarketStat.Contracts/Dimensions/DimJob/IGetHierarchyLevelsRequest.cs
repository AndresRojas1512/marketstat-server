namespace MarketStat.Contracts.Dimensions.DimJob;

public interface IGetHierarchyLevelsRequest
{
    int? IndustryFieldId { get; }
    string? StandardJobRoleTitle { get; }
}
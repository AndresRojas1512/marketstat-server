using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;

namespace MarketStat.Contracts.Dimensions.DimJob;

public interface IGetDimJobResponse
{
    int JobId { get; }
    string JobRoleTitle { get; }
    string StandardJobRoleTitle { get; }
    string HierarchyLevelName { get; }
    int IndustryFieldId { get; }
    DimIndustryFieldDto? IndustryField { get; }
}
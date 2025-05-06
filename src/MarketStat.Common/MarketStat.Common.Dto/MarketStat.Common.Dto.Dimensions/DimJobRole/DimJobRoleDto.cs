namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJobRole;

public record DimJobRoleDto(
    int JobRoleId,
    string JobRoleTitle,
    int StandardJobRoleId,
    int HierarchyLevelId
);
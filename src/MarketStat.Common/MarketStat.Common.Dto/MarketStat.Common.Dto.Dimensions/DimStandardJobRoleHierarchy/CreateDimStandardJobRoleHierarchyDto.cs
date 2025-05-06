using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimStandardJobRoleHierarchy;

public record CreateDimStandardJobRoleHierarchyDto
{
    [Required]
    public int StandardJobRoleId { get; init; }
    
    [Required]
    public int HierarchyLevelId { get; init; }
}
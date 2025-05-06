using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimHierarchyLevel;

public record CreateDimHierarchyLevelDto
{
    [Required]
    [MaxLength(255)]
    public string HierarchyLevelName { get; init; } = default!;
}
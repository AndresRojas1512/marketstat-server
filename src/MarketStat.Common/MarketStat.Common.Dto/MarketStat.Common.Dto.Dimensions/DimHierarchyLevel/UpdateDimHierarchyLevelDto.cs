using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimHierarchyLevel;

public record UpdateDimHierarchyLevelDto
{
    [Required]
    [MaxLength(255)]
    public string HierarchyLevelName { get; init; } = default!;
}
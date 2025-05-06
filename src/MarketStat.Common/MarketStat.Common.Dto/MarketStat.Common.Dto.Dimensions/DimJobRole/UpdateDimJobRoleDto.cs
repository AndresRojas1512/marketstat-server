using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJobRole;

public record UpdateDimJobRoleDto
{
    [Required]
    [MaxLength(255)]
    public string JobRoleTitle { get; init; } = default!;
    
    [Required]
    public int StandardJobRoleId { get; init; }
    
    [Required]
    public int HierarchyLevelId { get; init; }
}
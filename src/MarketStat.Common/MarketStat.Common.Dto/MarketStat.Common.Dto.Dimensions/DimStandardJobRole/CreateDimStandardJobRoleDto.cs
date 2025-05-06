using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimStandardJobRole;

public record CreateDimStandardJobRoleDto
{
    [Required]
    [MaxLength(255)]
    public string StandardJobRoleTitle { get; init; } = default!;
    
    [Required]
    public int IndustryFieldId { get; init; }
}
using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducationLevel;

public record CreateDimEducationLevelDto
{
    [Required]
    [MaxLength(255)]
    public string EducationLevelName { get; init; } = default!;
}
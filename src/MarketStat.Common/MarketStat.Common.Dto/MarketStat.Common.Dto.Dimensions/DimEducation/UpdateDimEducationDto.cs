using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducation;

public record UpdateDimEducationDto
{
    [Required]
    [MaxLength(255)]
    public string Specialty { get; init; } = default!;

    [Required]
    [MaxLength(255)]
    public string SpecialtyCode { get; init; } = default!;
    
    [Required]
    public int EducationLevelId { get; init; }
}
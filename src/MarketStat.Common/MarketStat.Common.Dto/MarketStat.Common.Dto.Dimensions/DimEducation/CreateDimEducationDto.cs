namespace MarketStat.Common.Dto.Dimensions.DimEducation;

using System.ComponentModel.DataAnnotations;

public record CreateDimEducationDto
{
    [Required]
    [MaxLength(255)]
    public string SpecialtyName { get; init; } = default!;

    [Required]
    [MaxLength(255)]
    public string SpecialtyCode { get; init; } = default!;

    [Required]
    [MaxLength(255)]
    public string EducationLevelName { get; init; } = default!;
}

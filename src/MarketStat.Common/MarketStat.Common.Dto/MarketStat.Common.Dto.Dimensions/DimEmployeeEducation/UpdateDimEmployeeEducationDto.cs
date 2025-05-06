using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployeeEducation;

public record UpdateDimEmployeeEducationDto
{
    [Required]
    [Range(1900, 2100)]
    public short GraduationYear { get; init; }
}
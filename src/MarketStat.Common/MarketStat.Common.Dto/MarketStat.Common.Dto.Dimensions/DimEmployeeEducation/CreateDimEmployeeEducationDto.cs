using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployeeEducation;

public record CreateDimEmployeeEducationDto
{
    [Required]
    public int EmployeeId { get; init; }
    
    [Required]
    public int EducationId { get; init; }
    
    [Required]
    [Range(1900, 2100)]
    public short GraduationYear { get; init; }
}
using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployee;

public record CreateDimEmployeeDto
{
    [Required]
    public DateOnly BirthDate { get; init; }
    
    [Required]
    public DateOnly CareerStartDate { get; init; }
}
using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployerIndustryField;

public record CreateDimEmployerIndustryFieldDto
{
    [Required]
    public int EmployerId { get; init; }
    
    [Required]
    public int IndustryFieldId { get; init; }
}
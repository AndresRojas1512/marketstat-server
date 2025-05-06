using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;

public class CreateDimEmployerDto
{
    [Required]
    [MaxLength(255)]
    public string EmployerName { get; init; } = default!;
    
    [Required]
    public bool IsPublic { get; init; }
}
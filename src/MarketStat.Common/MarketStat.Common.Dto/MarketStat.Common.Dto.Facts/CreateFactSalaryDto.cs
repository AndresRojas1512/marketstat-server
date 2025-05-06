using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public record CreateFactSalaryDto
{
    [Required]
    public int DateId { get; init; }
    
    [Required]
    public int CityId { get; init; }
    
    [Required]
    public int EmployerId { get; init; }
    
    [Required]
    public int JobRoleId { get; init; }
    
    [Required]
    public int EmployeeId { get; init; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal SalaryAmount { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal BonusAmount { get; init; } = 0m;
}
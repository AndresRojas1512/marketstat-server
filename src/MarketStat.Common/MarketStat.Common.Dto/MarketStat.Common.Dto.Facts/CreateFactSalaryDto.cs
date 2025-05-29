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
    
    [Required(ErrorMessage = "Salary amount is required.")]
    [Range(0.0, (double)decimal.MaxValue, ErrorMessage = "Salary amount must be a non-negative value.")]
    public decimal SalaryAmount { get; init; }

    [Required(ErrorMessage = "Bonus amount is required.")]
    [Range(0.0, (double)decimal.MaxValue, ErrorMessage = "Bonus amount must be a non-negative value.")]
    public decimal BonusAmount { get; init; } = 0m;
}
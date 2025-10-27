using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public record UpdateFactSalaryDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int DateId { get; init; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int LocationId { get; init; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployerId { get; init; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int JobId { get; init; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; init; }
    
    [Required]
    [Range(0.0, (double)decimal.MaxValue, ErrorMessage = "Salary amount must be a non-negative value.")]
    public decimal SalaryAmount { get; init; }
}
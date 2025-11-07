using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;

public record CreateFactSalaryDto
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
    
    [Required(ErrorMessage = "Salary amount is required.")]
    [Range(0.0, (double)decimal.MaxValue, ErrorMessage = "Salary amount must be a non-negative value.")]
    public decimal SalaryAmount { get; init; }
}
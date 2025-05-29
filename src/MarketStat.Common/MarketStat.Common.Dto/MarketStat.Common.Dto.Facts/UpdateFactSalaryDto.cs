using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public record UpdateFactSalaryDto
{
    [Required(ErrorMessage = "Date ID is required for update.")]
    public int DateId { get; init; }
    
    [Required(ErrorMessage = "City ID is required for update.")]
    public int CityId { get; init; }
    
    [Required(ErrorMessage = "Employer ID is required for update.")]
    public int EmployerId { get; init; }
    
    [Required(ErrorMessage = "Job Role ID is required for update.")]
    public int JobRoleId { get; init; }
    
    [Required(ErrorMessage = "Employee ID is required for update.")]
    public int EmployeeId { get; init; }
    
    [Required(ErrorMessage = "Salary amount is required for update.")]
    [Range(0.0, (double)decimal.MaxValue, ErrorMessage = "Salary amount must be a non-negative value.")]
    public decimal SalaryAmount { get; init; }

    [Required(ErrorMessage = "Bonus amount is required for update.")]
    [Range(0.0, (double)decimal.MaxValue, ErrorMessage = "Bonus amount must be a non-negative value.")]
    public decimal BonusAmount { get; init; }
}
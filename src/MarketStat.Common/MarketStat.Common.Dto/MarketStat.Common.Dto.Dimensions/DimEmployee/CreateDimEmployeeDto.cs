using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployee;

public class CreateDimEmployeeDto
{
    [Required(ErrorMessage = "EmployeeRefId is required.")]
    [StringLength(255, ErrorMessage = "EmployeeRefId cannot exceed 255 characters.")]
    public string EmployeeRefId { get; set; } = string.Empty;

    [Required(ErrorMessage = "BirthDate is required.")]
    public DateOnly BirthDate { get; set; }

    [Required(ErrorMessage = "CareerStartDate is required.")]
    public DateOnly CareerStartDate { get; set; }

    [StringLength(50, ErrorMessage = "Gender cannot exceed 50 characters.")]
    public string? Gender { get; set; }
}
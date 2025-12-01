namespace MarketStat.Common.Dto.Dimensions.DimEmployee;

using System.ComponentModel.DataAnnotations;

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

    [Range(1, int.MaxValue, ErrorMessage = "EducationId must be a positive number if provided.")]
    public int? EducationId { get; set; }

    [Range(1900, 2100, ErrorMessage = "GraduationYear must be a valid year if provided.")]
    public short? GraduationYear { get; set; }
}

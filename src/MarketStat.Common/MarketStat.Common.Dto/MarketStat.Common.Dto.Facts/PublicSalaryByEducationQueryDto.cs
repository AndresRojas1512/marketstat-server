using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class PublicSalaryByEducationQueryDto
{
    [Required(ErrorMessage = "IndustryFieldId is a required parameter.")]
    [Range(1, int.MaxValue, ErrorMessage = "IndustryFieldId must be a positive integer.")]
    public int IndustryFieldId { get; set; }

    [Range(1, 100, ErrorMessage = "TopNSpecialties must be between 1 and 100.")]
    public int TopNSpecialties { get; set; } = 10;

    [Range(0, int.MaxValue, ErrorMessage = "MinEmployeesPerSpecialty cannot be negative.")]
    public int MinEmployeesPerSpecialty { get; set; } = 5;

    [Range(0, int.MaxValue, ErrorMessage = "MinEmployeesPerLevelInSpecialty cannot be negative.")]
    public int MinEmployeesPerLevelInSpecialty { get; set; } = 3;

    public PublicSalaryByEducationQueryDto() { }
}
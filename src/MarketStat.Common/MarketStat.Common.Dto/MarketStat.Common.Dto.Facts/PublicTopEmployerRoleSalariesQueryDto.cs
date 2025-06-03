using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class PublicTopEmployerRoleSalariesQueryDto
{
    [Required(ErrorMessage = "IndustryFieldId is a required parameter.")]
    [Range(1, int.MaxValue, ErrorMessage = "IndustryFieldId must be a positive integer.")]
    public int IndustryFieldId { get; set; }

    [Range(1, 50, ErrorMessage = "TopNEmployers must be between 1 and 50.")]
    public int TopNEmployers { get; set; } = 5;

    [Range(1, 20, ErrorMessage = "TopMRolesPerEmployer must be between 1 and 20.")]
    public int TopMRolesPerEmployer { get; set; } = 3;

    [Range(0, int.MaxValue, ErrorMessage = "MinSalaryRecordsForRoleAtEmployer cannot be negative.")]
    public int MinSalaryRecordsForRoleAtEmployer { get; set; } = 3;

    public PublicTopEmployerRoleSalariesQueryDto() { }
}
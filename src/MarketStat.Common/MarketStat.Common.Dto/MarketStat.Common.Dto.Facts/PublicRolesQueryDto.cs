using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class PublicRolesQueryDto
{
    [Required(ErrorMessage = "IndustryFieldId is a required parameter.")]
    [Range(1, int.MaxValue, ErrorMessage = "IndustryFieldId must be a positive integer.")]
    public int IndustryFieldId { get; set; } 

    public int? FederalDistrictId { get; set; }
    public int? OblastId { get; set; }
    public int? CityId { get; set; }
            
    [Range(0, int.MaxValue, ErrorMessage = "MinSalaryRecordsForRole cannot be negative.")]
    public int MinSalaryRecordsForRole { get; set; } = 3;

    public PublicRolesQueryDto() { }
}
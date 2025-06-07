using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimStandardJobRole;

public class CreateDimStandardJobRoleDto
{
    [Required(ErrorMessage = "StandardJobRoleCode is required.")]
    [StringLength(20, ErrorMessage = "StandardJobRoleCode cannot exceed 20 characters.")]
    public string StandardJobRoleCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "StandardJobRoleTitle is required.")]
    [StringLength(255, ErrorMessage = "StandardJobRoleTitle cannot exceed 255 characters.")]
    public string StandardJobRoleTitle { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "IndustryFieldId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "IndustryFieldId must be a positive integer.")]
    public int IndustryFieldId { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;

public class UpdateDimIndustryFieldDto
{
    [Required(ErrorMessage = "IndustryFieldCode is required.")]
    [StringLength(10, ErrorMessage = "IndustryFieldCode cannot exceed 10 characters.")]
    public string IndustryFieldCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "IndustryFieldName is required.")]
    [StringLength(255, ErrorMessage = "IndustryFieldName cannot exceed 255 characters.")]
    public string IndustryFieldName { get; set; } = string.Empty;
}
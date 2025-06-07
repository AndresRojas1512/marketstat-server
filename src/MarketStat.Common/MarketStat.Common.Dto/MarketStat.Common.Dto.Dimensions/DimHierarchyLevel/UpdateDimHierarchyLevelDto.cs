using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimHierarchyLevel;

public class UpdateDimHierarchyLevelDto
{
    [Required(ErrorMessage = "HierarchyLevelCode is required.")]
    [StringLength(10, ErrorMessage = "HierarchyLevelCode cannot exceed 10 characters.")]
    public string HierarchyLevelCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "HierarchyLevelName is required.")]
    [StringLength(255, ErrorMessage = "HierarchyLevelName cannot exceed 255 characters.")]
    public string HierarchyLevelName { get; set; } = string.Empty;
}
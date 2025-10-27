using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJob;

public class UpdateDimJobDto
{
    [Required, StringLength(255)]
    public string JobRoleTitle { get; set; } = string.Empty;
    
    [Required, StringLength(255)]
    public string StandardJobRoleTitle { get; set; } = string.Empty;
    
    [Required, StringLength(255)]
    public string HierarchyLevelName { get; set; } = string.Empty;
    
    [Required, Range(1, int.MaxValue)]
    public int IndustryFieldId { get; set; }
}
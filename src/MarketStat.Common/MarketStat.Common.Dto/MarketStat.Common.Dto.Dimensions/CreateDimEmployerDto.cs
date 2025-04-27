namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;

public class CreateDimEmployerDto
{
    public string EmployerName { get; set; } = default!;
    public string Industry { get; set; } = default!;
    public bool IsPublic { get; set; }
}
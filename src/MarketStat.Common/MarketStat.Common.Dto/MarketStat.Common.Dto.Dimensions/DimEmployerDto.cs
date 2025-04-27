namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;

public record DimEmployerDto(
    int EmployerId,
    string EmployerName,
    string Industry,
    bool IsPublic
    );
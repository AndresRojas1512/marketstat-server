namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployee;

public record DimEmployeeDto(
    int EmployeeId,
    DateOnly BirthDate,
    DateOnly CareerStartDate
);
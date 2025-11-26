using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

namespace MarketStat.Contracts.Facts;

public interface IGetFactSalariesByFilterResponse
{
    List<FactSalaryDto> Salaries { get; }
}
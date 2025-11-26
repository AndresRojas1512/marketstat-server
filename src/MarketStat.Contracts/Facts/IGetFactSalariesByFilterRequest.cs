using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;

namespace MarketStat.Contracts.Facts;

public interface IGetFactSalariesByFilterRequest
{
    SalaryFilterDto Filter { get; }
}
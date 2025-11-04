using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Tests.TestData.Builders.Dimensions;

public class DimDateBuilder
{
    private int _dateId = 0;
    private DateOnly _fullDate = new(2025, 1, 1);

    public DimDateBuilder WithId(int id)
    {
        _dateId = id;
        return this;
    }

    public DimDateBuilder WithFullDate(DateOnly fullDate)
    {
        _fullDate = fullDate;
        return this;
    }

    public DimDateBuilder WithFullDate(int year, int month, int day)
    {
        _fullDate = new DateOnly(year, month, day);
        return this;
    }

    public DimDate Build()
    {
        var month = _fullDate.Month;
        var quarter = (month - 1) / 3 + 1;
        return new DimDate(
            _dateId,
            _fullDate,
            _fullDate.Year,
            quarter,
            month
        );
    }
}
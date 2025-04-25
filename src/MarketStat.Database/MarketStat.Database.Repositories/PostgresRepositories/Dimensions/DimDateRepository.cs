using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimDateRepository : IDimDateRepository
{
    private readonly Dictionary<int, DimDate> _dates = new Dictionary<int, DimDate>();
    
    public Task AddDateAsync(DimDate date)
    {
        if (!_dates.TryAdd(date.DateId, date))
        {
            throw new ArgumentException($"Date {date.DateId} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task<DimDate> GetDateByIdAsync(int dateId)
    {
        if (_dates.TryGetValue(dateId, out var d))
        {
            return Task.FromResult(d);
        }
        throw new KeyNotFoundException($"Employer {dateId} not found.");
    }

    public Task<IEnumerable<DimDate>> GetAllDatesAsync()
    {
        return Task.FromResult<IEnumerable<DimDate>>(_dates.Values);
    }

    public Task UpdateDateAsync(DimDate date)
    {
        if (!_dates.ContainsKey(date.DateId))
        {
            throw new KeyNotFoundException($"Cannot update: date {date.DateId} not found.");
        }
        _dates[date.DateId] = date;
        return Task.CompletedTask;
    }

    public Task DeleteDateAsync(int dateId)
    {
        if (!_dates.ContainsKey(dateId))
        {
            throw new KeyNotFoundException($"Cannot delete: date {dateId} not found.");
        }
        return Task.CompletedTask;
    }
}
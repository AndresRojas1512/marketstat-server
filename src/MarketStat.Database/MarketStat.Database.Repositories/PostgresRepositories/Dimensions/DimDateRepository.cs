using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimDateRepository : BaseRepository, IDimDateRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimDateRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddDateAsync(DimDate date)
    {
        var dbModel = new DimDateDbModel(
            dateId: 0,
            fullDate: date.FullDate,
            year: date.Year,
            quarter: date.Quarter,
            month: date.Month
        );
        await _dbContext.DimDates.AddAsync(dbModel);
        await _dbContext.SaveChangesAsync();
        date.DateId = dbModel.DateId;
    }

    public async Task<DimDate> GetDateByIdAsync(int dateId)
    {
        var dbDate = await _dbContext.DimDates.FindAsync(dateId) 
                     ?? throw new KeyNotFoundException($"Date {dateId} not found.");
        return DimDateConverter.ToDomain(dbDate);
    }

    public async Task<IEnumerable<DimDate>> GetAllDatesAsync()
    {
        var allDbDates = await _dbContext.DimDates.ToListAsync();
        return allDbDates.Select(DimDateConverter.ToDomain);
    }

    public async Task UpdateDateAsync(DimDate date)
    {
        var dbDate = await _dbContext.DimDates.FindAsync(date.DateId) 
                     ?? throw new KeyNotFoundException($"Cannot update Date {date.DateId}.");
        dbDate.DateId = date.DateId;
        dbDate.FullDate = date.FullDate;
        dbDate.Year = date.Year;
        dbDate.Quarter = date.Quarter;
        dbDate.Month = date.Month;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteDateAsync(int dateId)
    {
        var dbDate = await _dbContext.DimDates.FindAsync(dateId) 
                     ?? throw new KeyNotFoundException($"Cannot delete Date {dateId}.");
        _dbContext.DimDates.Remove(dbDate);
        await _dbContext.SaveChangesAsync();
    }
}
using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"A date for {date.FullDate:yyyy-MM-dd} already exists.");
        }
        date.DateId = dbModel.DateId;
    }

    public async Task<DimDate> GetDateByIdAsync(int dateId)
    {
        var dbDate = await _dbContext.DimDates.FindAsync(dateId);
        if (dbDate is null)
            throw new NotFoundException($"Date with ID {dateId} not found.");
        return DimDateConverter.ToDomain(dbDate);
    }

    public async Task<IEnumerable<DimDate>> GetAllDatesAsync()
    {
        var allDbDates = await _dbContext.DimDates.ToListAsync();
        return allDbDates.Select(DimDateConverter.ToDomain);
    }

    public async Task UpdateDateAsync(DimDate date)
    {
        var dbDate = await _dbContext.DimDates.FindAsync(date.DateId);
        if (dbDate is null)
            throw new NotFoundException($"Date with ID {date.DateId} not found.");
        
        dbDate.FullDate = date.FullDate;
        dbDate.Year = date.Year;
        dbDate.Quarter = date.Quarter;
        dbDate.Month = date.Month;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"A date for {date.FullDate:yyyy-MM-dd} already exists.");
        }
    }

    public async Task DeleteDateAsync(int dateId)
    {
        var dbDate = await _dbContext.DimDates.FindAsync(dateId);
        if (dbDate is null)
            throw new NotFoundException($"Date with ID {dateId} not found.");
        _dbContext.DimDates.Remove(dbDate);
        await _dbContext.SaveChangesAsync();
    }
}
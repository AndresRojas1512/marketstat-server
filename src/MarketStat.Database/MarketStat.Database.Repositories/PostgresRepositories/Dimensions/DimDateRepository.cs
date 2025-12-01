namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

using MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public class DimDateRepository : BaseRepository, IDimDateRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimDateRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddDateAsync(DimDate dimDate)
    {
        ArgumentNullException.ThrowIfNull(dimDate);
        var dbModel = DimDateConverter.ToDbModel(dimDate);
        await _dbContext.DimDates.AddAsync(dbModel).ConfigureAwait(false);
        try
        {
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"A date for {dimDate.FullDate:yyyy-MM-dd} already exists.");
        }

        dimDate.DateId = dbModel.DateId;
    }

    public async Task<DimDate> GetDateByIdAsync(int dateId)
    {
        var dbDate = await _dbContext.DimDates.FindAsync(dateId).ConfigureAwait(false);
        if (dbDate is null)
        {
            throw new NotFoundException($"Date with ID {dateId} not found.");
        }

        return DimDateConverter.ToDomain(dbDate);
    }

    public async Task<IEnumerable<DimDate>> GetAllDatesAsync()
    {
        var allDbDates = await _dbContext.DimDates.ToListAsync().ConfigureAwait(false);
        return allDbDates.Select(DimDateConverter.ToDomain);
    }

    public async Task UpdateDateAsync(DimDate dimDate)
    {
        ArgumentNullException.ThrowIfNull(dimDate);
        var dbDate = await _dbContext.DimDates.FindAsync(dimDate.DateId).ConfigureAwait(false);
        if (dbDate is null)
        {
            throw new NotFoundException($"Date with ID {dimDate.DateId} not found.");
        }

        dbDate.FullDate = dimDate.FullDate;
        dbDate.Year = dimDate.Year;
        dbDate.Quarter = dimDate.Quarter;
        dbDate.Month = dimDate.Month;

        try
        {
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"A date for {dimDate.FullDate:yyyy-MM-dd} already exists.");
        }
    }

    public async Task DeleteDateAsync(int dateId)
    {
        var dbDate = await _dbContext.DimDates.FindAsync(dateId).ConfigureAwait(false);
        if (dbDate is null)
        {
            throw new NotFoundException($"Date with ID {dateId} not found.");
        }

        _dbContext.DimDates.Remove(dbDate);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}

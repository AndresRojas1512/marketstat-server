using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimOblastRepository : BaseRepository, IDimOblastRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimOblastRepository(MarketStatDbContext context)
    {
        _dbContext = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task AddOblastAsync(DimOblast dimOblast)
    {
        var dbModel = new DimOblastDbModel(
            oblastId: 0,
            oblastName: dimOblast.OblastName,
            districtId: dimOblast.DistrictId
        );
        await _dbContext.DimOblasts.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"An oblast with name '{dimOblast.OblastName}' already exists.");
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                $"Referenced federal district ({dimOblast.DistrictId}) not found.");
        }
        dimOblast.OblastId = dbModel.OblastId;
    }

    public async Task<DimOblast> GetOblastByIdAsync(int id)
    {
        var dbOblast = await _dbContext.DimOblasts.FindAsync(id);
        if (dbOblast is null)
            throw new NotFoundException($"Oblast {id} not found");
        return DimOblastConverter.ToDomain(dbOblast);
    }

    public async Task<IEnumerable<DimOblast>> GetAllOblastsAsync()
    {
        var allDbOblasts = await _dbContext.DimOblasts.ToListAsync();
        return allDbOblasts.Select(DimOblastConverter.ToDomain);
    }

    public async Task<IEnumerable<DimOblast>> GetOblastsByFederalDistrictIdAsync(int id)
    {
        var dbOblasts = await _dbContext.DimOblasts
            .Where(o => o.DistrictId == id)
            .ToListAsync();
        return dbOblasts.Select(DimOblastConverter.ToDomain);
    }

    public async Task UpdateOblastAsync(DimOblast dimOblast)
    {
        var dbOblast = await _dbContext.DimOblasts.FindAsync(dimOblast.OblastId);
        if (dbOblast is null)
            throw new NotFoundException($"Oblast {dimOblast.OblastId} not found");
        
        dbOblast.OblastName = dimOblast.OblastName;
        dbOblast.DistrictId = dimOblast.DistrictId;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"An oblast with name '{dimOblast.OblastName}' already exists.");
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                $"Referenced federal district ({dimOblast.DistrictId}) not found.");
        }
    }

    public async Task DeleteOblastAsync(int id)
    {
        var dbOblast = await _dbContext.DimOblasts.FindAsync(id);
        if (dbOblast is null)
            throw new NotFoundException($"Oblast {id} not found");
        _dbContext.DimOblasts.Remove(dbOblast);
        await _dbContext.SaveChangesAsync();
    }
}
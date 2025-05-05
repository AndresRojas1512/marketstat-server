using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;

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
        await _dbContext.SaveChangesAsync();
        dimOblast.OblastId = dbModel.OblastId;
    }

    public async Task<DimOblast> GetOblastByIdAsync(int id)
    {
        var dbOblast = await _dbContext.DimOblasts.FindAsync(id) 
                       ?? throw new KeyNotFoundException($"Oblast {id} not found");
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
        var dbOblast = await _dbContext.DimOblasts.FindAsync(dimOblast.OblastId) 
                       ?? throw new KeyNotFoundException($"Cannot update Oblast {dimOblast.OblastId}.");
        dbOblast.OblastName = dimOblast.OblastName;
        dbOblast.DistrictId = dimOblast.DistrictId;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteOblastAsync(int id)
    {
        var dbOblast = await _dbContext.DimOblasts.FindAsync(id) 
                       ?? throw new KeyNotFoundException($"Cannot delete Oblast {id}.");
        _dbContext.DimOblasts.Remove(dbOblast);
        await _dbContext.SaveChangesAsync();
    }
}
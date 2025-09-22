using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimFederalDistrictRepository : BaseRepository, IDimFederalDistrictRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimFederalDistrictRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddFederalDistrictAsync(DimFederalDistrict district)
    {
        var dbModel = DimFederalDistrictConverter.ToDbModel(district);
        await _dbContext.DimFederalDistricts.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"A federal district named '{district.DistrictName}' already exists.");
        }
        district.DistrictId = dbModel.DistrictId;
    }

    public async Task<DimFederalDistrict> GetFederalDistrictByIdAsync(int id)
    {
        var dbDistrict = await _dbContext.DimFederalDistricts.FindAsync(id);
        if (dbDistrict is null)
            throw new NotFoundException($"Federal district with {id} not found.");
        return DimFederalDistrictConverter.ToDomain(dbDistrict);
    }

    public async Task<IEnumerable<DimFederalDistrict>> GetAllFederalDistrictsAsync()
    {
        var dbAllDistricts = await _dbContext.DimFederalDistricts.ToListAsync();
        return dbAllDistricts.Select(DimFederalDistrictConverter.ToDomain);
    }

    public async Task UpdateFederalDistrictAsync(DimFederalDistrict district)
    {
        var dbDistrict = await _dbContext.DimFederalDistricts.FindAsync(district.DistrictId);
        if (dbDistrict is null)
            throw new NotFoundException($"Federal district with {district.DistrictId} not found.");
        
        dbDistrict.DistrictName = district.DistrictName;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"A federal district named '{district.DistrictName}' already exists.");
        }
    }

    public async Task DeleteFederalDistrictAsync(int id)
    {
        var dbDistrict = await _dbContext.DimFederalDistricts.FindAsync(id);
        if (dbDistrict is null)
            throw new NotFoundException($"Cannot delete: FederalDistrict {id} not found.");
        _dbContext.DimFederalDistricts.Remove(dbDistrict);
        await _dbContext.SaveChangesAsync();
    }
}
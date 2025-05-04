using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;

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
        var dbModel = new DimFederalDistrictDbModel(
            districtId: 0,
            districtName: district.DistrictName
        );
        await _dbContext.DimFederalDistricts.AddAsync(dbModel);
        await _dbContext.SaveChangesAsync();
        district.DistrictId = dbModel.DistrictId;
    }

    public async Task<DimFederalDistrict> GetFederalDistrictByIdAsync(int id)
    {
        var dbDistrict = await _dbContext.DimFederalDistricts.FindAsync(id) 
                         ?? throw new KeyNotFoundException($"FederalDistrict {id} not found.");
        return DimFederalDistrictConverter.ToDomain(dbDistrict);
    }

    public async Task<IEnumerable<DimFederalDistrict>> GetAllFederalDistrictsAsync()
    {
        var dbAllDistricts = await _dbContext.DimFederalDistricts.ToListAsync();
        return dbAllDistricts.Select(DimFederalDistrictConverter.ToDomain);
    }

    public async Task UpdateFederalDistrictAsync(DimFederalDistrict district)
    {
        var dbDistrict = await _dbContext.DimFederalDistricts.FindAsync(district.DistrictId) 
                         ?? throw new KeyNotFoundException($"Cannot update: FederalDistrict {district.DistrictId} not found.");
        dbDistrict.DistrictName = district.DistrictName;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteFederalDistrictAsync(int id)
    {
        var dbDistrict = await _dbContext.DimFederalDistricts.FindAsync(id) 
                         ?? throw new KeyNotFoundException($"Cannot delete: FederalDistrict {id} not found.");
        _dbContext.DimFederalDistricts.Remove(dbDistrict);
        await _dbContext.SaveChangesAsync();
    }
}
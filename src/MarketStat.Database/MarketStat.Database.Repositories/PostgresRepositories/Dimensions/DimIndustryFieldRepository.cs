using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimIndustryFieldRepository : BaseRepository, IDimIndustryFieldRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimIndustryFieldRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddIndustryFieldAsync(DimIndustryField industryField)
    {
        var dbModel = new DimIndustryFieldDbModel(
            industryFieldId: 0,
            industryFieldName: industryField.IndustryFieldName
        );
        await _dbContext.DimIndustryFields.AddAsync(dbModel);
        await _dbContext.SaveChangesAsync();
        industryField.IndustryFieldId = dbModel.IndustryFieldId;
    }

    public async Task<DimIndustryField> GetIndustryFieldByIdAsync(int industryFieldId)
    {
        var dbIndustryField = await _dbContext.DimIndustryFields.FindAsync(industryFieldId) 
                              ?? throw new KeyNotFoundException($"Industry field with id {industryFieldId} not found");
        return DimIndustryFieldConverter.ToDomain(dbIndustryField);
    }

    public async Task<IEnumerable<DimIndustryField>> GetAllIndustryFieldsAsync()
    {
        var allDbIndustryFields = await _dbContext.DimIndustryFields.ToListAsync();
        return allDbIndustryFields.Select(DimIndustryFieldConverter.ToDomain);
    }

    public async Task UpdateIndustryFieldAsync(DimIndustryField industryField)
    {
        var dbIndustryField = await _dbContext.DimIndustryFields.FindAsync(industryField.IndustryFieldId) 
                              ?? throw new KeyNotFoundException($"Cannot update {industryField.IndustryFieldId}.");
        dbIndustryField.IndustryFieldName = industryField.IndustryFieldName;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteIndustryFieldAsync(int industryFieldId)
    {
        var dbIndustryField = await _dbContext.DimIndustryFields.FindAsync(industryFieldId)
                              ?? throw new KeyNotFoundException($"Cannot delete {industryFieldId}.");
        _dbContext.DimIndustryFields.Remove(dbIndustryField);
        await _dbContext.SaveChangesAsync();
    }
}
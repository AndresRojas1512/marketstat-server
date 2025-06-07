using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        var dbModel = DimIndustryFieldConverter.ToDbModel(industryField);
            
        await _dbContext.DimIndustryFields.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx 
                  && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException(
                $"An industry field with the code '{industryField.IndustryFieldCode}' or name '{industryField.IndustryFieldName}' already exists.");
        }
        industryField.IndustryFieldId = dbModel.IndustryFieldId;
    }

    public async Task<DimIndustryField> GetIndustryFieldByIdAsync(int industryFieldId)
    {
        var dbIndustryField = await _dbContext.DimIndustryFields
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.IndustryFieldId == industryFieldId);
                
        if (dbIndustryField == null)
        {
            throw new NotFoundException($"Industry field with id {industryFieldId} not found.");
        }
        return DimIndustryFieldConverter.ToDomain(dbIndustryField);
    }

    public async Task<IEnumerable<DimIndustryField>> GetAllIndustryFieldsAsync()
    {
        var allDbIndustryFields = await _dbContext.DimIndustryFields
            .AsNoTracking()
            .OrderBy(i => i.IndustryFieldName)
            .ToListAsync();
        return allDbIndustryFields.Select(DimIndustryFieldConverter.ToDomain);
    }

    public async Task UpdateIndustryFieldAsync(DimIndustryField industryField)
    {
        var dbIndustryField = await _dbContext.DimIndustryFields
            .FirstOrDefaultAsync(i => i.IndustryFieldId == industryField.IndustryFieldId);

        if (dbIndustryField == null)
        {
            throw new NotFoundException($"Industry field with id {industryField.IndustryFieldId} not found");
        }
            
        dbIndustryField.IndustryFieldName = industryField.IndustryFieldName;
        dbIndustryField.IndustryFieldCode = industryField.IndustryFieldCode;
            
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx 
                  && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"Updating resulted in a conflict. The code '{industryField.IndustryFieldCode}' or name '{industryField.IndustryFieldName}' may already exist.");
        }
    }

    public async Task DeleteIndustryFieldAsync(int industryFieldId)
    {
        var dbIndustryField = await _dbContext.DimIndustryFields.FindAsync(industryFieldId);
        if (dbIndustryField == null)
        {
            throw new NotFoundException($"Industry field with id {industryFieldId} not found");
        }
        _dbContext.DimIndustryFields.Remove(dbIndustryField);
        await _dbContext.SaveChangesAsync();
    }
}
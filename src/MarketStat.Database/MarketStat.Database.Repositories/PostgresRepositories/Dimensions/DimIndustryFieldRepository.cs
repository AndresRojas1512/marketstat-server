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
        var dbModel = new DimIndustryFieldDbModel(
            industryFieldId: 0,
            industryFieldName: industryField.IndustryFieldName
        );
        await _dbContext.DimIndustryFields.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"An industry field '{industryField.IndustryFieldName}' already exists.");
        }
        industryField.IndustryFieldId = dbModel.IndustryFieldId;
    }

    public async Task<DimIndustryField> GetIndustryFieldByIdAsync(int industryFieldId)
    {
        var dbIndustryField = await _dbContext.DimIndustryFields.FindAsync(industryFieldId);
        if (dbIndustryField is null)
            throw new NotFoundException($"Industry field with id {industryFieldId} not found.");
        return DimIndustryFieldConverter.ToDomain(dbIndustryField);
    }

    public async Task<IEnumerable<DimIndustryField>> GetAllIndustryFieldsAsync()
    {
        var allDbIndustryFields = await _dbContext.DimIndustryFields.ToListAsync();
        return allDbIndustryFields.Select(DimIndustryFieldConverter.ToDomain);
    }

    public async Task UpdateIndustryFieldAsync(DimIndustryField industryField)
    {
        var dbIndustryField = await _dbContext.DimIndustryFields.FindAsync(industryField.IndustryFieldId);
        if (dbIndustryField is null)
            throw new NotFoundException($"Industry field with id {industryField.IndustryFieldId} not found");
        dbIndustryField.IndustryFieldName = industryField.IndustryFieldName;
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"An industry field with code '{industryField.IndustryFieldName}' already exists.");
        }
    }

    public async Task DeleteIndustryFieldAsync(int industryFieldId)
    {
        var dbIndustryField = await _dbContext.DimIndustryFields.FindAsync(industryFieldId);
        if (dbIndustryField is null)
            throw new NotFoundException($"Industry field with id {industryFieldId} not found");
        _dbContext.DimIndustryFields.Remove(dbIndustryField);
        await _dbContext.SaveChangesAsync();
    }
}
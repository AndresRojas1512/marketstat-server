using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEmployerIndustryFieldRepository : BaseRepository, IDimEmployerIndustryFieldRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimEmployerIndustryFieldRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddEmployerIndustryFieldAsync(DimEmployerIndustryField link)
    {
        var dbLink = DimEmployerIndustryFieldConverter.ToDbModel(link);
        await _dbContext.DimEmployerIndustryFields.AddAsync(dbLink);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"The link employer {link.EmployerId} & industry field {link.IndustryFieldId} already exists.");
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException($"Either employer {link.EmployerId} or industry field {link.IndustryFieldId} does not exist.");
        }
    }

    public async Task<DimEmployerIndustryField> GetEmployerIndustryFieldAsync(int employerId, int industryFieldId)
    {
        var dbLink = await _dbContext.DimEmployerIndustryFields.FindAsync(employerId, industryFieldId);
        if (dbLink is null)
            throw new NotFoundException($"Link employer {employerId} & industry field {industryFieldId}) not found.");
        return DimEmployerIndustryFieldConverter.ToDomain(dbLink);
    }

    public async Task<IEnumerable<DimEmployerIndustryField>> GetIndustryFieldsByEmployerIdAsync(int employerId)
    {
        var dbLinks = await _dbContext.DimEmployerIndustryFields
            .Where(e => e.EmployerId == employerId)
            .ToListAsync();
        return dbLinks.Select(DimEmployerIndustryFieldConverter.ToDomain);
    }

    public async Task<IEnumerable<DimEmployerIndustryField>> GetEmployersByIndustryFieldIdAsync(int industryFieldId)
    {
        var dbLinks = await _dbContext.DimEmployerIndustryFields
            .Where(i => i.IndustryFieldId == industryFieldId)
            .ToListAsync();
        return dbLinks.Select(DimEmployerIndustryFieldConverter.ToDomain);
    }

    public async Task<IEnumerable<DimEmployerIndustryField>> GetAllEmployerIndustryFieldsAsync()
    {
        var dbLinks = await _dbContext.DimEmployerIndustryFields.ToListAsync();
        return dbLinks.Select(DimEmployerIndustryFieldConverter.ToDomain);
    }

    public async Task DeleteEmployerIndustryFieldAsync(int employerId, int industryFieldId)
    {
        var dbLink = await _dbContext.DimEmployerIndustryFields.FindAsync(employerId, industryFieldId);
        if (dbLink is null)
            throw new NotFoundException($"Link employer {employerId} & industry field {industryFieldId}) not found.");
        _dbContext.DimEmployerIndustryFields.Remove(dbLink);
        await _dbContext.SaveChangesAsync();
    }
}
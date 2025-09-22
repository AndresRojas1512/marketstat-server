using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimStandardJobRoleRepository : BaseRepository, IDimStandardJobRoleRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimStandardJobRoleRepository(MarketStatDbContext context)
    {
        _dbContext = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task AddStandardJobRoleAsync(DimStandardJobRole jobRole)
    {
        var dbModel = DimStandardJobRoleConverter.ToDbModel(jobRole);
        await _dbContext.DimStandardJobRoles.AddAsync(dbModel);
            
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ConflictException($"A standard job role with the code '{jobRole.StandardJobRoleCode}' or title '{jobRole.StandardJobRoleTitle}' already exists.");
            }
            if (pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                throw new NotFoundException($"The specified IndustryFieldId {jobRole.IndustryFieldId} does not exist.");
            }
            throw;
        }
            
        jobRole.StandardJobRoleId = dbModel.StandardJobRoleId;
    }

    public async Task<DimStandardJobRole> GetStandardJobRoleByIdAsync(int id)
    {
        var dbJobRole = await _dbContext.DimStandardJobRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(sjr => sjr.StandardJobRoleId == id);
            
        if (dbJobRole == null)
        {
            throw new NotFoundException($"Standard job role with ID {id} not found.");
        }
        return DimStandardJobRoleConverter.ToDomain(dbJobRole);
    }

    public async Task<IEnumerable<DimStandardJobRole>> GetAllStandardJobRolesAsync()
    {
        var allJobRoles = await _dbContext.DimStandardJobRoles
            .AsNoTracking()
            .OrderBy(sjr => sjr.StandardJobRoleTitle)
            .ToListAsync();
        return allJobRoles.Select(DimStandardJobRoleConverter.ToDomain);
    }

    public async Task UpdateStandardJobRoleAsync(DimStandardJobRole jobRole)
    {
        var dbJobRole = await _dbContext.DimStandardJobRoles
            .FirstOrDefaultAsync(sjr => sjr.StandardJobRoleId == jobRole.StandardJobRoleId);

        if (dbJobRole == null)
        {
            throw new NotFoundException($"Standard job role with ID {jobRole.StandardJobRoleId} not found.");
        }
    
        dbJobRole.StandardJobRoleTitle = jobRole.StandardJobRoleTitle;
        dbJobRole.StandardJobRoleCode = jobRole.StandardJobRoleCode;
        dbJobRole.IndustryFieldId = jobRole.IndustryFieldId;
    
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ConflictException($"Updating resulted in a conflict. The code '{jobRole.StandardJobRoleCode}' or title '{jobRole.StandardJobRoleTitle}' may already exist.");
            }
            if (pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                throw new NotFoundException($"The specified IndustryFieldId {jobRole.IndustryFieldId} does not exist.");
            }
            throw;
        }
    }

    public async Task DeleteStandardJobRoleAsync(int id)
    {
        var dbJobRole = await _dbContext.DimStandardJobRoles.FindAsync(id);
        if (dbJobRole == null)
        {
            throw new NotFoundException($"Standard job role with ID {id} not found.");
        }
        _dbContext.DimStandardJobRoles.Remove(dbJobRole);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<DimStandardJobRole>> GetStandardJobRolesByIndustryAsync(int industryFieldId)
    {
        if (industryFieldId <= 0)
        {
            return Enumerable.Empty<DimStandardJobRole>();
        }

        var dbJobRoles = await _dbContext.DimStandardJobRoles
            .Where(sjr => sjr.IndustryFieldId == industryFieldId)
            .AsNoTracking()
            .OrderBy(sjr => sjr.StandardJobRoleTitle)
            .ToListAsync();
        return dbJobRoles.Select(DimStandardJobRoleConverter.ToDomain);
    }
}
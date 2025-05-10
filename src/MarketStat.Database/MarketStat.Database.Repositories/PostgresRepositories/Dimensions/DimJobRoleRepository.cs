using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimJobRoleRepository : BaseRepository, IDimJobRoleRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimJobRoleRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddJobRoleAsync(DimJobRole jobRole)
    {
        var dbModel = new DimJobRoleDbModel(
            jobRoleId: 0,
            jobRoleTitle: jobRole.JobRoleTitle,
            standardJobRoleId: jobRole.StandardJobRoleId,
            hierarchyLevelId: jobRole.HierarchyLevelId
        );
        await _dbContext.DimJobRoles.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"A job role '{jobRole.JobRoleTitle}' already exists.");
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                $"Referenced standard job role ({jobRole.StandardJobRoleId}) or hierarchy level ({jobRole.HierarchyLevelId}) not found.");
        }
        jobRole.JobRoleId = dbModel.JobRoleId;
    }

    public async Task<DimJobRole> GetJobRoleByIdAsync(int jobRoleId)
    {
        var dbJobRole = await _dbContext.DimJobRoles.FindAsync(jobRoleId);
        if (dbJobRole is null)
            throw new NotFoundException($"Job role {jobRoleId} not found.");
        return DimJobRoleConverter.ToDomain(dbJobRole);
    }

    public async Task<IEnumerable<DimJobRole>> GetAllJobRolesAsync()
    {
        var allDbJobRoles = await _dbContext.DimJobRoles.ToListAsync();
        return allDbJobRoles.Select(DimJobRoleConverter.ToDomain);
    }

    public async Task UpdateJobRoleAsync(DimJobRole jobRole)
    {
        var dbJobRole = await _dbContext.DimJobRoles.FindAsync(jobRole.JobRoleId);
        if (dbJobRole is null)
            throw new NotFoundException($"Job role {jobRole.JobRoleId} not found.");
        
        dbJobRole.JobRoleTitle = jobRole.JobRoleTitle;
        dbJobRole.StandardJobRoleId = jobRole.StandardJobRoleId;
        dbJobRole.HierarchyLevelId = jobRole.HierarchyLevelId;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"A job role '{jobRole.JobRoleTitle}' already exists.");
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                $"Referenced standard job role ({jobRole.StandardJobRoleId}) or hierarchy level ({jobRole.HierarchyLevelId}) not found.");
        }
    }

    public async Task DeleteJobRoleAsync(int jobRoleId)
    {
        var dbJobRole = await _dbContext.DimJobRoles.FindAsync(jobRoleId);
        if (dbJobRole is null)
            throw new NotFoundException($"Job role {jobRoleId} not found.");
        _dbContext.DimJobRoles.Remove(dbJobRole);
        await _dbContext.SaveChangesAsync();
    }
}
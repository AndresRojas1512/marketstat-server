using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimJobRepository : BaseRepository, IDimJobRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimJobRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddJobAsync(DimJob job)
    {
        var dbModel = DimJobConverter.ToDbModel(job);
        await _dbContext.DimJobs.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException is PostgresException pg &&
                                             pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException("A job with the same titles, hierarchy, and industry already exists.");
        }
        job.JobId = dbModel.JobId;
    }

    public async Task<DimJob> GetJobByIdAsync(int jobId)
    {
        var dbJob = await _dbContext.DimJobs.Include(j => j.IndustryField).AsNoTracking().FirstOrDefaultAsync(j => j.JobId == jobId);
        if (dbJob is null)
            throw new NotFoundException($"Job with ID {jobId} not found.");
        return DimJobConverter.ToDomain(dbJob);
    }

    public async Task<IEnumerable<DimJob>> GetAllJobsAsync()
    {
        var allDbJobs = await _dbContext.DimJobs.Include(j => j.IndustryField).AsNoTracking()
            .OrderBy(j => j.StandardJobRoleTitle).ToListAsync();
        return allDbJobs.Select(DimJobConverter.ToDomain);
    }

    public async Task UpdateJobAsync(DimJob job)
    {
        var dbJob = await _dbContext.DimJobs.FindAsync(job.JobId);
        if (dbJob is null)
            throw new NotFoundException($"Job with ID {job.JobId} not found.");

        dbJob.JobRoleTitle = job.JobRoleTitle;
        dbJob.StandardJobRoleTitle = job.StandardJobRoleTitle;
        dbJob.HierarchyLevelName = job.HierarchyLevelName;
        dbJob.IndustryFieldId = job.IndustryFieldId;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException is PostgresException pg &&
                                             pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException("Updating resulted in a conflict. The job may already exist.");
        }
    }

    public async Task DeleteJobAsync(int jobId)
    {
        var dbJob = await _dbContext.DimJobs.FindAsync(jobId);
        if (dbJob is null)
            throw new NotFoundException($"Job with ID {jobId} not found.");
        _dbContext.DimJobs.Remove(dbJob);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<int>> GetJobIdsByFilterAsync(string? standardJobRoleTitle, string? hierarchyLevelName,
        int? industryFieldId)
    {
        var query = _dbContext.DimJobs.AsQueryable();
        if (industryFieldId.HasValue)
            query = query.Where(j => j.IndustryFieldId == industryFieldId);
        if (!string.IsNullOrEmpty(standardJobRoleTitle))
            query = query.Where(j => j.StandardJobRoleTitle == standardJobRoleTitle);
        if (!string.IsNullOrEmpty(hierarchyLevelName))
            query = query.Where(j => j.HierarchyLevelName == hierarchyLevelName);
        return await query.Select(j => j.JobId).ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDistinctStandardJobRolesAsync(int? industryFieldId)
    {
        var query = _dbContext.DimJobs.AsQueryable();
        if (industryFieldId.HasValue)
        {
            query = query.Where(j => j.IndustryFieldId == industryFieldId.Value);
        }
        return await query
            .Select(j => j.StandardJobRoleTitle)
            .Distinct()
            .OrderBy(title => title)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDistinctHierarchyLevelsAsync(int? industryFieldId,
        string? standardJobRoleTitle)
    {
        var query = _dbContext.DimJobs.AsQueryable();
        if (industryFieldId.HasValue)
        {
            query = query.Where(j => j.IndustryFieldId == industryFieldId.Value);
        }
        if (!string.IsNullOrEmpty(standardJobRoleTitle))
        {
            query = query.Where(j => j.StandardJobRoleTitle == standardJobRoleTitle);
        }
        return await query
            .Select(j => j.HierarchyLevelName)
            .Distinct()
            .OrderBy(level => level)
            .ToListAsync();
    }
}
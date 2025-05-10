using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimStandardJobRoleHierarchyRepository : BaseRepository, IDimStandardJobRoleHierarchyRepository
{
    private readonly MarketStatDbContext _context;

    public DimStandardJobRoleHierarchyRepository(MarketStatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task AddStandardJobRoleHierarchyAsync(DimStandardJobRoleHierarchy link)
    {
        var dbLink = DimStandardJobRoleHierarchyConverter.ToDbModel(link);
        await _context.DimStandardJobRoleHierarchies.AddAsync(dbLink);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException(
                $"Link ({link.StandardJobRoleId}, {link.HierarchyLevelId}) already exists.");
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                $"Either StandardJobRole {link.StandardJobRoleId} or HierarchyLevel {link.HierarchyLevelId} does not exist.");
        }
    }

    public async Task<DimStandardJobRoleHierarchy> GetStandardJobRoleHierarchyAsync(int jobRoleId, int levelId)
    {
        var dbLink = await _context.DimStandardJobRoleHierarchies.FindAsync(jobRoleId, levelId);
        if (dbLink is null) throw new NotFoundException($"Link ({jobRoleId}, {levelId}) not found.");
        return DimStandardJobRoleHierarchyConverter.ToDomain(dbLink);
    }

    public async Task<IEnumerable<DimStandardJobRoleHierarchy>> GetLevelsByJobRoleIdAsync(int jobRoleId)
    {
        var dbList = await _context.DimStandardJobRoleHierarchies
            .Where(j => j.StandardJobRoleId == jobRoleId)
            .ToListAsync();
        return dbList.Select(DimStandardJobRoleHierarchyConverter.ToDomain);
    }

    public async Task<IEnumerable<DimStandardJobRoleHierarchy>> GetJobRolesByLevelIdAsync(int levelId)
    {
        var dbList = await _context.DimStandardJobRoleHierarchies
            .Where(l => l.HierarchyLevelId == levelId)
            .ToListAsync();
        return dbList.Select(DimStandardJobRoleHierarchyConverter.ToDomain);
    }

    public async Task<IEnumerable<DimStandardJobRoleHierarchy>> GetAllStandardJobRoleHierarchiesAsync()
    {
        var dbList = await _context.DimStandardJobRoleHierarchies.ToListAsync();
        return dbList.Select(DimStandardJobRoleHierarchyConverter.ToDomain);
    }

    public async Task DeleteStandardJobRoleHierarchyAsync(int jobRoleId, int levelId)
    {
        var dbLink = await _context.DimStandardJobRoleHierarchies.FindAsync(jobRoleId, levelId);
        if (dbLink is null)
            throw new NotFoundException($"Link ({jobRoleId}, {levelId}) not found.");
        _context.DimStandardJobRoleHierarchies.Remove(dbLink);
        await _context.SaveChangesAsync();
    }
}
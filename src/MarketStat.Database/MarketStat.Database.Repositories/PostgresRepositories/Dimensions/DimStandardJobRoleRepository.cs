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
    private readonly MarketStatDbContext _context;

    public DimStandardJobRoleRepository(MarketStatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task AddStandardJobRoleAsync(DimStandardJobRole jobRole)
    {
        var dbModel = new DimStandardJobRoleDbModel(
            standardJobRoleId: 0,
            standardJobRoleTitle: jobRole.StandardJobRoleTitle,
            industryFieldId: jobRole.IndustryFieldId
        );
        await _context.DimStandardJobRoles.AddAsync(dbModel);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException(
                $"A job role '{jobRole.StandardJobRoleTitle}' in industry field {jobRole.IndustryFieldId} already exists.");
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                $"IndustryField with ID {jobRole.IndustryFieldId} not found.");
        }

        jobRole.StandardJobRoleId = dbModel.StandardJobRoleId;
    }

    public async Task<DimStandardJobRole> GetStandardJobRoleByIdAsync(int id)
    {
        var dbJobRole = await _context.DimStandardJobRoles.FindAsync(id);
        if (dbJobRole is null)
            throw new NotFoundException($"Standard job role with ID {id} not found.");
        return DimStandardJobRoleConverter.ToDomain(dbJobRole);
    }

    public async Task<IEnumerable<DimStandardJobRole>> GetAllStandardJobRolesAsync()
    {
        var allJobRoles = await _context.DimStandardJobRoles.ToListAsync();
        return allJobRoles.Select(DimStandardJobRoleConverter.ToDomain);
    }

    public async Task UpdateStandardJobRoleAsync(DimStandardJobRole jobRole)
    {
        var dbJobRole = await _context.DimStandardJobRoles.FindAsync(jobRole.StandardJobRoleId);
        if (dbJobRole is null)
            throw new NotFoundException($"Standard job role with ID {jobRole.StandardJobRoleId} not found.");
        
        dbJobRole.StandardJobRoleTitle = jobRole.StandardJobRoleTitle;
        dbJobRole.IndustryFieldId = jobRole.IndustryFieldId;
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException(
                $"A job role '{jobRole.StandardJobRoleTitle}' in industry field {jobRole.IndustryFieldId} already exists.");
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                $"IndustryField with ID {jobRole.IndustryFieldId} not found.");
        }
    }

    public async Task DeleteStandardJobRoleAsync(int id)
    {
        var dbJobRole = await _context.DimStandardJobRoles.FindAsync(id);
        if (dbJobRole is null)
            throw new NotFoundException($"Standard job role with ID {id} not found.");
        _context.DimStandardJobRoles.Remove(dbJobRole);
        await _context.SaveChangesAsync();
    }
}
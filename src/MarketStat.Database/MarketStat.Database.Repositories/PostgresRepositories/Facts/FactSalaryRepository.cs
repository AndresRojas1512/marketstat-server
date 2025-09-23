using System.Data;
using System.Text;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System.Linq;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Etl;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;

namespace MarketStat.Database.Repositories.PostgresRepositories.Facts;

public class FactSalaryRepository : IFactSalaryRepository
{
    private readonly MarketStatDbContext _dbContext;
    private readonly ILogger<FactSalaryRepository> _logger;

    public FactSalaryRepository(MarketStatDbContext dbContext, ILogger<FactSalaryRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task AddFactSalaryAsync(FactSalary salary)
    {
        var dbModel = FactSalaryConverter.ToDbModel(salary);
        await _dbContext.FactSalaries.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx &&
                  pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                "One or more referenced entities (date, city, employer, job role or employee) were not found when adding salary fact.");
        }
        salary.SalaryFactId = dbModel.SalaryFactId;
    }

    public async Task<FactSalary> GetFactSalaryByIdAsync(long salaryId)
    {
        var dbSalary = await _dbContext.FactSalaries
            .AsNoTracking()
            .FirstOrDefaultAsync(fs => fs.SalaryFactId == salaryId);

        if (dbSalary == null)
        {
            throw new NotFoundException($"Salary fact with ID {salaryId} not found.");
        }
        return FactSalaryConverter.ToDomain(dbSalary);
    }

    public async Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync()
    {
        var allDbModels = await _dbContext.FactSalaries.AsNoTracking().ToListAsync();
        return allDbModels.Select(FactSalaryConverter.ToDomain);
    }

    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto filterDto)
    {
        _logger.LogInformation("Repository: GetFactSalariesByFilterAsync called with filter: {@FilterDto}", filterDto);

        try
        {
            var query = GetFilteredSalariesQuery(filterDto);
            var dbModels = await query.AsNoTracking().ToListAsync();
            _logger.LogInformation("Repository: Successfully retrieved {Count} salary facts using LINQ.",
                dbModels.Count);
            return dbModels.Select(FactSalaryConverter.ToDomain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Repository: Generic error executing LINQ query for filter: {@FilterDto}", filterDto);
            throw new ApplicationException("An error occurred while processing filtered salaries.", ex);
        }
    }

    public async Task UpdateFactSalaryAsync(FactSalary salaryFact)
    {
        var dbModel = await _dbContext.FactSalaries.FirstOrDefaultAsync(fs => fs.SalaryFactId == salaryFact.SalaryFactId);
        if (dbModel == null)
            throw new NotFoundException($"Salary fact with ID {salaryFact.SalaryFactId} not found for update.");

        dbModel.DateId = salaryFact.DateId;
        dbModel.CityId = salaryFact.CityId;
        dbModel.EmployerId = salaryFact.EmployerId;
        dbModel.JobRoleId = salaryFact.JobRoleId;
        dbModel.EmployeeId = salaryFact.EmployeeId;
        dbModel.SalaryAmount = salaryFact.SalaryAmount;
        dbModel.BonusAmount = salaryFact.BonusAmount;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx &&
                  pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                "One or more referenced entities (date, city, employer, job role or employee) were not found during update.");
        }
    }

    public async Task DeleteFactSalaryByIdAsync(long salaryFactId)
    {
        var dbModel = await _dbContext.FactSalaries.FindAsync(salaryFactId);
        if (dbModel == null)
            throw new NotFoundException($"Salary fact with ID {salaryFactId} not found for deletion.");

        _dbContext.FactSalaries.Remove(dbModel);
        await _dbContext.SaveChangesAsync();
    }
    
    // Auth

    public async Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(SalaryFilterDto filters)
    {
        var baseQuery = GetFilteredSalariesQuery(filters);
        var salaries = await baseQuery
            .Select(f => f.SalaryAmount)
            .ToListAsync();
        if (salaries.Count == 0)
        {
            return new List<SalaryDistributionBucketDto>();
        }

        var n = salaries.Count;
        var minVal = salaries.Min();
        var maxVal = salaries.Max();

        if (n <= 1 || minVal == maxVal)
        {
            return new List<SalaryDistributionBucketDto>
            {
                new SalaryDistributionBucketDto { LowerBound = minVal, UpperBound = maxVal = n }
            };
        }

        int bucketCount = (int)Math.Floor(Math.Log(n, 2)) + 2;
        decimal delta = (maxVal - minVal) / bucketCount;

        if (delta == 0)
        {
            return new List<SalaryDistributionBucketDto>
            {
                new SalaryDistributionBucketDto { LowerBound = minVal, UpperBound = maxVal, BucketCount = n }
            };
        }

        var distribution = salaries
            .GroupBy(salary => (int)Math.Floor((salary - minVal) / delta))
            .Select(g =>
            {
                int bucketNo = Math.Clamp(g.Key, 0, bucketCount - 1);
                var lowerBound = minVal + (bucketNo * delta);
                var upperBound = lowerBound + delta;

                return new
                {
                    LowerBound = lowerBound,
                    UpperBound = upperBound,
                    Count = g.Count()
                };
            })
            .OrderBy(b => b.LowerBound)
            .ToList();

        return distribution.Select(b => new SalaryDistributionBucketDto
        {
            LowerBound = b.LowerBound,
            UpperBound = b.UpperBound,
            BucketCount = b.Count
        }).ToList();
    }

    public async Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile)
    {
        var baseQuery = GetFilteredSalariesQuery(filters);
        var summaryStats = await baseQuery
            .GroupBy(f => 1)
            .Select(g => new
            {
                TotalCount = g.Count(),
                AverageSalary = g.Average(f => f.SalaryAmount)
            })
            .FirstOrDefaultAsync();
        if (summaryStats == null || summaryStats.TotalCount == 0)
        {
            return null;
        }
        
        var salariesForPercentile = await baseQuery
            .OrderBy(f => f.SalaryAmount)
            .Select(f => f.SalaryAmount)
            .ToListAsync();

        var result = new SalarySummaryDto
        {
            TotalCount = summaryStats.TotalCount,
            AverageSalary = summaryStats.AverageSalary,
            Percentile25 = CalculatePercentile(salariesForPercentile, 25),
            Percentile50 = CalculatePercentile(salariesForPercentile, 50),
            Percentile75 = CalculatePercentile(salariesForPercentile, 75),
            PercentileTarget = CalculatePercentile(salariesForPercentile, targetPercentile),
        };
        return result;
    }

    public async Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters, TimeGranularity granularity, int periods)
    {
        var referenceDate = filters.DateEnd ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var seriesEndDate = GetPeriodStartDate(referenceDate, granularity);
        var seriesStartDate = AddPeriods(seriesEndDate, granularity, -(periods - 1));

        var baseQuery = GetFilteredSalariesQuery(filters)
            .Where(f => f.DimDate.FullDate >= seriesStartDate &&
                        f.DimDate.FullDate < AddPeriods(seriesEndDate, granularity, 1));

        var dbResults = await baseQuery
            .GroupBy(f => GetPeriodStartDate(f.DimDate.FullDate, granularity))
            .Select(g => new
            {
                PeriodStart = g.Key,
                AvgSalary = g.Average(f => f.SalaryAmount),
                SalaryCountInPeriod = g.Count()
            })
            .ToDictionaryAsync(r => r.PeriodStart);
        
        var allPeriods = new List<SalaryTimeSeriesPointDto>();
        var currentPeriod = seriesStartDate;
        for (int i = 0; i < periods; i++)
        {
            if (dbResults.TryGetValue(currentPeriod, out var stats))
            {
                allPeriods.Add(new SalaryTimeSeriesPointDto
                {
                    PeriodStart = currentPeriod,
                    AvgSalary = stats.AvgSalary,
                    SalaryCountInPeriod = stats.SalaryCountInPeriod
                });
            }
            else
            {
                allPeriods.Add(new SalaryTimeSeriesPointDto
                {
                    PeriodStart = currentPeriod,
                    AvgSalary = 0,
                    SalaryCountInPeriod = 0
                });
            }

            currentPeriod = AddPeriods(currentPeriod, granularity, 1);
        }
        return allPeriods.OrderBy(p => p.PeriodStart).ToList();
    }
    
    // =========================
    // Public analytical methods
    // =========================
    
    public async Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesByLocationIndustryAsync(PublicRolesQueryDto queryDto)
    {
        _logger.LogInformation(
            "Repository (LINQ): GetPublicRolesByLocationIndustryAsync with DTO: {@QueryDto}", queryDto);
        var periodEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1).AddDays(-DateTime.UtcNow.Day));
        var periodStartDate = periodEndDate.AddMonths(-12).AddDays(1);

        var filters = new SalaryFilterDto
        {
            IndustryFieldId = queryDto.IndustryFieldId,
            DistrictId = queryDto.FederalDistrictId,
            OblastId = queryDto.OblastId,
            CityId = queryDto.CityId,
            DateStart = periodStartDate,
            DateEnd = periodEndDate
        };
        
        var query = GetFilteredSalariesQuery(filters);
        var results = await query
            .GroupBy(fs => fs.DimJobRole.DimStandardJobRole.StandardJobRoleTitle)
            .Select(g => new
            {
                StandardJobRoleTitle = g.Key,
                AverageSalary = g.Average(fs => fs.SalaryAmount),
                SalaryRecordCount = g.Count()
            })
            .Where(g => g.SalaryRecordCount >= queryDto.MinSalaryRecordsForRole)
            .OrderByDescending(g => g.AverageSalary)
            .ThenByDescending(g => g.SalaryRecordCount)
            .ThenBy(g => g.StandardJobRoleTitle)
            .Select(g => new PublicRoleByLocationIndustryDto
            {
                StandardJobRoleTitle = g.StandardJobRoleTitle,
                AverageSalary = g.AverageSalary,
                SalaryRecordCount = g.SalaryRecordCount
            })
            .AsNoTracking()
            .ToListAsync();
        _logger.LogInformation("Repository (LINQ): GetPublicRolesByLocationIndustryAsync returned {Count} records.", results.Count);
        return results;
    }
    
    public async Task<IEnumerable<PublicSalaryByEducationInIndustryDto>> GetPublicSalaryByEducationInIndustryAsync(PublicSalaryByEducationQueryDto queryDto)
    {
        _logger.LogInformation("Repository (LINQ): GetPublicSalaryByEducationInIndustryAsync with DTO: {@QueryDto}", queryDto);
        var periodEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1).AddDays(-DateTime.UtcNow.Day));
        var periodStartDate = periodEndDate.AddMonths(-12).AddDays(1);

        var baseQuery = _dbContext.FactSalaries
            .Include(fs => fs.DimDate)
            .Include(fs => fs.DimJobRole).ThenInclude(jr => jr.DimStandardJobRole)
            .Include(fs => fs.DimEmployee).ThenInclude(e => e.DimEmployeeEducations)
            .ThenInclude(ee => ee.Education).ThenInclude(ed => ed.DimEducationLevel)
            .Where(fs => fs.DimJobRole.DimStandardJobRole.IndustryFieldId == queryDto.IndustryFieldId)
            .Where(fs => fs.DimDate.FullDate >= periodStartDate && fs.DimDate.FullDate <= periodEndDate);

        var specialtyLevelStats = await baseQuery
            .SelectMany(fs => fs.DimEmployee.DimEmployeeEducations, (fs, ee) => new
            {
                Salary = fs.SalaryAmount,
                EmployeeId = fs.EmployeeId,
                Specialty = ee.Education.Specialty,
                EducationLevel = ee.Education.DimEducationLevel.EducationLevelName
            })
            .GroupBy(x => new { x.Specialty, x.EducationLevel })
            .Select(g => new
            {
                g.Key.Specialty,
                g.Key.EducationLevel,
                AverageSalary = g.Average(x => x.Salary),
                EmployeeCount = g.Select(x => x.EmployeeId).Distinct().Count()
            })
            .Where(x => x.EmployeeCount >= queryDto.MinEmployeesPerLevelInSpecialty)
            .ToListAsync();

        var topSpecialties = specialtyLevelStats
            .GroupBy(s => s.Specialty)
            .Select(g => new
            {
                Specialty = g.Key,
                OverallEmployeeCount = g.Sum(s => s.EmployeeCount)
            })
            .Where(g => g.OverallEmployeeCount >= queryDto.MinEmployeesPerSpecialty)
            .OrderByDescending(g => g.OverallEmployeeCount)
            .ThenBy(g => g.Specialty)
            .Take(queryDto.TopNSpecialties)
            .Select(g => g.Specialty)
            .ToHashSet();

        var finalResult = specialtyLevelStats
            .Where(s => topSpecialties.Contains(s.Specialty))
            .GroupBy(s => s.Specialty)
            .SelectMany(g => g.Select(levelStat => new PublicSalaryByEducationInIndustryDto
            {
                EducationSpecialty = levelStat.Specialty,
                EducationLevelName = levelStat.EducationLevel,
                AverageSalary = levelStat.AverageSalary,
                EmployeeCountForLevel = levelStat.EmployeeCount,
                OverallSpecialtyEmployeeCount = g.Sum(s => s.EmployeeCount)
            }))
            .OrderBy(r => Array.IndexOf(topSpecialties.ToArray(), r.EducationSpecialty))
            .ThenBy(r => r.EducationLevelName)
            .ToList();

        return finalResult;
    }

    public async Task<IEnumerable<PublicTopEmployerRoleSalariesInIndustryDto>> GetPublicTopEmployerRoleSalariesInIndustryAsync(
            PublicTopEmployerRoleSalariesQueryDto queryDto)
    {
        _logger.LogInformation("Repository (LINQ): GetPublicTopEmployerRoleSalariesInIndustryAsync with DTO: {@QueryDto}", queryDto);
        
        var periodEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1).AddDays(-DateTime.UtcNow.Day));
        var periodStartDate = periodEndDate.AddMonths(-12).AddDays(1);

        var baseQuery = _dbContext.FactSalaries
            .Include(fs => fs.DimDate)
            .Include(fs => fs.DimEmployer)
            .Include(fs => fs.DimJobRole).ThenInclude(jr => jr.DimStandardJobRole)
            .Where(fs => fs.DimJobRole.DimStandardJobRole.IndustryFieldId == queryDto.IndustryFieldId)
            .Where(fs => fs.DimDate.FullDate >= periodStartDate && fs.DimDate.FullDate <= periodEndDate);

        var topEmployers = await baseQuery
            .GroupBy(fs => new { fs.EmployerId, fs.DimEmployer.EmployerName })
            .Select(g => new
            {
                EmployerId = g.Key.EmployerId,
                EmployerName = g.Key.EmployerName,
                RecordCount = g.Count()
            })
            .OrderByDescending(x => x.RecordCount)
            .ThenBy(x => x.EmployerName)
            .Take(queryDto.TopNEmployers)
            .ToListAsync();

        var topEmployerIds = topEmployers.Select(e => e.EmployerId).ToList();

        var roleStatsForTopEmployers = await baseQuery
            .Where(fs => topEmployerIds.Contains(fs.EmployerId))
            .GroupBy(fs => new
            {
                fs.EmployerId,
                fs.DimJobRole.DimStandardJobRole.StandardJobRoleTitle
            })
            .Select(g => new
            {
                g.Key.EmployerId,
                g.Key.StandardJobRoleTitle,
                AverageSalaryForRole = g.Average(fs => fs.SalaryAmount),
                SalaryRecordCountForRole = g.Count()
            })
            .Where(g => g.SalaryRecordCountForRole >= queryDto.MinSalaryRecordsForRoleAtEmployer)
            .ToListAsync();
        
        var finalResult = topEmployers
            .Select((employer, index) => new { Employer = employer, Rank = index + 1 })
            .SelectMany(rankedEmployer => roleStatsForTopEmployers
                .Where(role => role.EmployerId == rankedEmployer.Employer.EmployerId)
                .OrderByDescending(role => role.SalaryRecordCountForRole)
                .ThenByDescending(role => role.AverageSalaryForRole)
                .ThenBy(role => role.StandardJobRoleTitle)
                .Take(queryDto.TopMRolesPerEmployer)
                .Select((role, roleIndex) => new PublicTopEmployerRoleSalariesInIndustryDto
                {
                    EmployerName = rankedEmployer.Employer.EmployerName,
                    StandardJobRoleTitle = role.StandardJobRoleTitle,
                    AverageSalaryForRole = role.AverageSalaryForRole,
                    SalaryRecordCountForRole = role.SalaryRecordCountForRole,
                    EmployerRank = rankedEmployer.Rank,
                    RoleRankWithinEmployer = roleIndex + 1
                })
            )
            .ToList();

        return finalResult;
    }
    
    // =====
    // Utils
    // =====

    private IQueryable<FactSalaryDbModel> GetFilteredSalariesQuery(SalaryFilterDto filter)
    {
        var query = _dbContext.FactSalaries
            .Include(fs => fs.DimDate)
            .Include(fs => fs.DimCity).ThenInclude(c => c.DimOblast).ThenInclude(o => o.DimFederalDistrict)
            .Include(fs => fs.DimJobRole).ThenInclude(jr => jr.DimStandardJobRole).ThenInclude(sjr => sjr.DimIndustryField)
            .Include(fs => fs.DimJobRole).ThenInclude(jr => jr.DimHierarchyLevel)
            .AsQueryable();
        if (filter.IndustryFieldId.HasValue)
        {
            query = query.Where(fs =>
                fs.DimJobRole.DimStandardJobRole.IndustryFieldId == filter.IndustryFieldId.Value);
        }

        if (filter.StandardJobRoleId.HasValue)
        {
            query = query.Where(fs => fs.DimJobRole.StandardJobRoleId == filter.StandardJobRoleId.Value);
        }

        if (filter.HierarchyLevelId.HasValue)
        {
            query = query.Where(fs => fs.DimJobRole.HierarchyLevelId == filter.HierarchyLevelId.Value);
        }

        if (filter.DistrictId.HasValue)
        {
            query = query.Where(fs => fs.DimCity.DimOblast.DistrictId == filter.DistrictId.Value);
        }

        if (filter.OblastId.HasValue)
        {
            query = query.Where(fs => fs.DimCity.OblastId == filter.OblastId.Value);
        }

        if (filter.CityId.HasValue)
        {
            query = query.Where(fs => fs.CityId == filter.CityId.Value);
        }

        if (filter.DateStart.HasValue)
        {
            query = query.Where(fs => fs.DimDate.FullDate >= filter.DateStart.Value);
        }

        if (filter.DateEnd.HasValue)
        {
            query = query.Where(fs => fs.DimDate.FullDate <= filter.DateEnd.Value);
        }
        return query;
    }
    
    private DateOnly GetPeriodStartDate(DateOnly date, TimeGranularity granularity)
    {
        return granularity switch
        {
            TimeGranularity.Year => new DateOnly(date.Year, 1, 1),
            TimeGranularity.Quarter => new DateOnly(date.Year, ((date.Month - 1) / 3) * 3 + 1, 1),
            _ => new DateOnly(date.Year, date.Month, 1),
        };
    }

    private DateOnly AddPeriods(DateOnly date, TimeGranularity granularity, int count)
    {
        return granularity switch
        {
            TimeGranularity.Year => date.AddYears(count),
            TimeGranularity.Quarter => date.AddMonths(count * 3),
            _ => date.AddMonths(count),
        };
    }
    
    private decimal? CalculatePercentile(List<decimal> sortedData, int percentile)
    {
        if (sortedData.Count == 0)
        {
            return null;
        }

        if (percentile <= 0)
        {
            return sortedData[0];
        }

        if (percentile >= 100)
        {
            return sortedData[sortedData.Count - 1];
        }
        
        double index = (percentile / 100.0) * (sortedData.Count - 1);
        int lowerIndex = (int)index;
        int upperIndex = lowerIndex + 1;
        
        if (upperIndex >= sortedData.Count)
        {
            return sortedData[lowerIndex];
        }
        double weight = index - lowerIndex;
        
        return (decimal)((1 - weight) * (double)sortedData[lowerIndex] + weight * (double)sortedData[upperIndex]);
    }
}
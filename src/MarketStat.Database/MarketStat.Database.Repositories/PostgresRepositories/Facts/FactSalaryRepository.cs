using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Responses;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Facts;

public class FactSalaryRepository : BaseRepository, IFactSalaryRepository
{
    private readonly MarketStatDbContext _dbContext;

    public FactSalaryRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }


    public async Task AddFactSalaryAsync(FactSalary salary)
    {
        var dbModel = FactSalaryConverter.ToDbModel(salary);
        await _dbContext.FactSalaries.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
            salary.SalaryFactId = dbModel.SalaryFactId;
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx &&
                  pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                "One or more referenced entities (date, city, employer, job role or employee) were not found when adding salary fact.");
        }
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

    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(ResolvedSalaryFilter resolvedFilters)
    {
        try
        {
            var query = GetFilteredSalariesQuery(resolvedFilters);
            var dbModels = await query.AsNoTracking().ToListAsync();
            return dbModels.Select(FactSalaryConverter.ToDomain);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while processing filtered salaries.", ex);
        }
    }

    public async Task UpdateFactSalaryAsync(FactSalary salaryFact)
    {
        var dbModel = await _dbContext.FactSalaries.FindAsync(salaryFact.SalaryFactId);
        if (dbModel == null)
            throw new NotFoundException($"Salary fact with ID {salaryFact.SalaryFactId} not found for update.");

        dbModel.DateId = salaryFact.DateId;
        dbModel.LocationId = salaryFact.LocationId;
        dbModel.EmployerId = salaryFact.EmployerId;
        dbModel.JobId = salaryFact.JobId;
        dbModel.EmployeeId = salaryFact.EmployeeId;
        dbModel.SalaryAmount = salaryFact.SalaryAmount;
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
    
    // Authorized analytical methods

    public async Task<List<SalaryDistributionBucket>> GetSalaryDistributionAsync(ResolvedSalaryFilter resolvedFilters)
    {
        var baseQuery = GetFilteredSalariesQuery(resolvedFilters);
        var salaries = await baseQuery
            .Select(f => f.SalaryAmount)
            .ToListAsync();
        if (salaries.Count == 0)
        {
            return new List<SalaryDistributionBucket>();
        }

        var n = salaries.Count;
        var minVal = salaries.Min();
        var maxVal = salaries.Max();

        if (n <= 1 || minVal == maxVal)
        {
            return new List<SalaryDistributionBucket>
            {
                new SalaryDistributionBucket { LowerBound = minVal, UpperBound = maxVal, BucketCount = n}
            };
        }

        int bucketCount = (int)Math.Max(2, Math.Floor(Math.Log(n, 2)) + 2);
        decimal delta = (maxVal - minVal) / bucketCount;

        if (delta == 0)
        {
            return new List<SalaryDistributionBucket>
            {
                new SalaryDistributionBucket { LowerBound = minVal, UpperBound = maxVal, BucketCount = n }
            };
        }

        var distribution = salaries
            .GroupBy(salary => (int)Math.Floor((salary - minVal) / delta))
            .Select(g =>
            {
                int bucketNo = Math.Clamp(g.Key, 0, bucketCount - 1);
                var lowerBound = minVal + (bucketNo * delta);
                var upperBound = (bucketNo == bucketCount - 1) ? maxVal : lowerBound + delta;

                return new
                {
                    BucketNo = bucketNo,
                    LowerBound = lowerBound,
                    UpperBound = upperBound,
                    Count = g.Count()
                };
            })
            .GroupBy(b => b.BucketNo)
            .Select(finalGroup => new SalaryDistributionBucket
            {
                LowerBound = finalGroup.First().LowerBound,
                UpperBound = finalGroup.First().UpperBound,
                BucketCount = finalGroup.Sum(item => item.Count)
            })
            .OrderBy(b => b.LowerBound)
            .ToList();
        
        return distribution;
    }

    public async Task<SalarySummary?> GetSalarySummaryAsync(ResolvedSalaryFilter resolvedFilters, int targetPercentile)
    {
        var baseQuery = GetFilteredSalariesQuery(resolvedFilters);
        
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

        List<decimal> salariesForPercentile = new List<decimal>();
        if (summaryStats.TotalCount > 0)
        {
            salariesForPercentile = await baseQuery
                .OrderBy(f => f.SalaryAmount)
                .Select(f => f.SalaryAmount)
                .ToListAsync();
        }

        var result = new SalarySummary
        {
            TotalCount = summaryStats.TotalCount,
            AverageSalary = summaryStats.AverageSalary,
            Percentile25 = CalculatePercentile(salariesForPercentile, 25),
            Percentile50 = CalculatePercentile(salariesForPercentile, 50),
            Percentile75 = CalculatePercentile(salariesForPercentile, 75),
            PercentileTarget = CalculatePercentile(salariesForPercentile, targetPercentile)
        };
        return result;
    }

    public async Task<List<SalaryTimeSeriesPoint>> GetSalaryTimeSeriesAsync(ResolvedSalaryFilter resolvedFilters,
        TimeGranularity granularity, int periods)
    {
        var referenceDate = resolvedFilters.DateEnd ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var seriesEndDate = GetPeriodStartDate(referenceDate, granularity);
        var seriesStartDate = AddPeriods(seriesEndDate, granularity, -(periods - 1));
        var overallEndDate = AddPeriods(seriesEndDate, granularity, 1);
        
        var baseQuery = GetFilteredSalariesQuery(resolvedFilters)
            .Where(f => f.DimDate!.FullDate >= seriesStartDate &&
                        f.DimDate.FullDate < overallEndDate);

        var dbResults = await baseQuery
            .GroupBy(f => GetPeriodStartDate(f.DimDate!.FullDate, granularity))
            .Select(g => new
            {
                PeriodStart = g.Key,
                AvgSalary = g.Average(f => f.SalaryAmount),
                SalaryCountInPeriod = g.Count()
            })
            .ToDictionaryAsync(r => r.PeriodStart);

        var allPeriods = new List<SalaryTimeSeriesPoint>();
        var currentPeriodStart = seriesStartDate;
        for (int i = 0; i < periods; i++)
        {
            if (dbResults.TryGetValue(currentPeriodStart, out var stats))
            {
                allPeriods.Add(new SalaryTimeSeriesPoint
                {
                    PeriodStart = currentPeriodStart,
                    AvgSalary = stats.AvgSalary,
                    SalaryCountInPeriod = stats.SalaryCountInPeriod
                });
            }
            else
            {
                allPeriods.Add(new SalaryTimeSeriesPoint
                {
                    PeriodStart = currentPeriodStart,
                    AvgSalary = 0,
                    SalaryCountInPeriod = 0
                });
            }

            currentPeriodStart = AddPeriods(currentPeriodStart, granularity, 1);
        }

        return allPeriods.OrderBy(p => p.PeriodStart).ToList();
    }

    // Public analytical methods

    public async Task<IEnumerable<PublicRoleByLocationIndustry>> GetPublicRolesAsync(
        ResolvedSalaryFilter resolvedFilters, int minRecordCount)
    {
        var query = GetFilteredSalariesQuery(resolvedFilters);
        var results = await query
            .Include(fs => fs.DimJob)
            .GroupBy(fs => fs.DimJob!.StandardJobRoleTitle)
            .Select(g => new PublicRoleByLocationIndustry
            {
                StandardJobRoleTitle = g.Key,
                AverageSalary = g.Average(fs => fs.SalaryAmount),
                SalaryRecordCount = g.Count()
            })
            .Where(g => g.SalaryRecordCount >= minRecordCount)
            .OrderByDescending(g => g.AverageSalary)
            .AsNoTracking()
            .ToListAsync();
        return results;
    }
    
    // Utils

    private IQueryable<FactSalaryDbModel> GetFilteredSalariesQuery(ResolvedSalaryFilter resolvedFilters)
    {
        var query = _dbContext.FactSalaries
            .Include(fs => fs.DimDate)
            .AsQueryable();

        if (resolvedFilters.LocationIds != null && resolvedFilters.LocationIds.Any())
        {
            query = query.Where(fs => resolvedFilters.LocationIds.Contains(fs.LocationId));
        }

        if (resolvedFilters.JobIds != null && resolvedFilters.JobIds.Any())
        {
            query = query.Where(fs => resolvedFilters.JobIds.Contains(fs.JobId));
        }

        if (resolvedFilters.DateStart.HasValue)
        {
            query = query.Where(fs => fs.DimDate!.FullDate >= resolvedFilters.DateStart.Value);
        }

        if (resolvedFilters.DateEnd.HasValue)
        {
            query = query.Where(fs => fs.DimDate!.FullDate <= resolvedFilters.DateEnd.Value);
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
        
        double realIndex = (percentile / 100.0) * (sortedData.Count - 1);
        int lowerIndex = (int)Math.Floor(realIndex);
        int upperIndex = (int)Math.Ceiling(realIndex);
        
        if (lowerIndex == upperIndex)
        {
            return sortedData[lowerIndex];
        }
        double weight = realIndex - lowerIndex;
        
        return (decimal)((1 - weight) * (double)sortedData[lowerIndex] + weight * (double)sortedData[upperIndex]);
    }
}
using System.Globalization;
using System.Text;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Responses;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;
using MarketStat.Common.Enums;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Facts;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Facts;

public class FactSalaryRepositoryEfSql : IFactSalaryRepository
{
    private readonly MarketStatDbContext _context;

    public FactSalaryRepositoryEfSql(MarketStatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task<List<SalaryDistributionBucket>> GetSalaryDistributionAsync(ResolvedSalaryFilter filter)
    {
        var (whereSql, sqlParams) = BuildWhereClause(filter);
        
        var sql = $@"
            WITH RawData AS (
                SELECT salary_amount
                FROM marketstat.fact_salaries fs
                LEFT JOIN marketstat.dim_date d ON fs.date_id = d.date_id
                WHERE {whereSql}
            ),
            Stats AS (
                SELECT MIN(salary_amount) as min_val, MAX(salary_amount) as max_val, COUNT(*) as total_count
                FROM RawData
            ),
            Config AS (
                SELECT 
                    min_val, max_val, total_count, -- FIX: Pass this through
                    GREATEST(2, FLOOR(LOG(2.0, GREATEST(total_count, 1))) + 2) as bucket_count
                FROM Stats
            ),
            BucketParams AS (
                SELECT 
                    min_val, max_val, bucket_count, total_count, -- FIX: Pass this through
                    CASE WHEN bucket_count > 0 THEN (max_val - min_val) / bucket_count ELSE 0 END as width
                FROM Config
            )
            SELECT 
                CAST(min_val + (LEAST(width_bucket(salary_amount, min_val, max_val, CAST(bucket_count AS INT)), CAST(bucket_count AS INT)) - 1) * width AS numeric) as ""lower_bound"",
                CAST(min_val + (LEAST(width_bucket(salary_amount, min_val, max_val, CAST(bucket_count AS INT)), CAST(bucket_count AS INT))) * width AS numeric) as ""upper_bound"",
                COUNT(*) as ""bucket_count""
            FROM RawData
            CROSS JOIN BucketParams
            WHERE total_count > 0 AND width > 0
            GROUP BY 1, 2
            ORDER BY 1";

        var dtos = await _context.Set<SalaryDistributionBucketDto>()
            .FromSqlRaw(sql, sqlParams.ToArray())
            .AsNoTracking()
            .ToListAsync();

        return dtos.Select(d => new SalaryDistributionBucket
        {
            LowerBound = d.LowerBound,
            UpperBound = d.UpperBound,
            BucketCount = d.BucketCount
        }).ToList();
    }

    public async Task<SalarySummary?> GetSalarySummaryAsync(ResolvedSalaryFilter filter, int targetPercentile)
    {
        var pVal = (targetPercentile / 100.0).ToString(CultureInfo.InvariantCulture);
        var (whereSql, sqlParams) = BuildWhereClause(filter);
        
        var sql = $@"
            SELECT 
                COUNT(*) as ""total_count"",
                CAST(COALESCE(AVG(salary_amount), 0) AS numeric) as ""average_salary"",
                CAST(PERCENTILE_CONT(0.25) WITHIN GROUP (ORDER BY salary_amount) AS numeric) as ""percentile25"",
                CAST(PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY salary_amount) AS numeric) as ""percentile50"",
                CAST(PERCENTILE_CONT(0.75) WITHIN GROUP (ORDER BY salary_amount) AS numeric) as ""percentile75"",
                CAST(PERCENTILE_CONT({pVal}) WITHIN GROUP (ORDER BY salary_amount) AS numeric) as ""percentile_target""
            FROM marketstat.fact_salaries fs
            LEFT JOIN marketstat.dim_date d ON fs.date_id = d.date_id
            WHERE {whereSql}";

        var resultDto = await _context.Set<SalarySummaryDto>()
            .FromSqlRaw(sql, sqlParams.ToArray())
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (resultDto == null) return null;

        return new SalarySummary
        {
            TotalCount = resultDto.TotalCount,
            AverageSalary = resultDto.AverageSalary ?? 0,
            Percentile25 = resultDto.Percentile25,
            Percentile50 = resultDto.Percentile50,
            Percentile75 = resultDto.Percentile75,
            PercentileTarget = resultDto.PercentileTarget
        };
    }

    public async Task<List<SalaryTimeSeriesPoint>> GetSalaryTimeSeriesAsync(ResolvedSalaryFilter filter, TimeGranularity granularity, int periods)
    {
        var (whereSql, sqlParams) = BuildWhereClause(filter);
        string truncPart = granularity switch
        {
            TimeGranularity.Year => "year",
            TimeGranularity.Quarter => "quarter",
            _ => "month"
        };

        var sql = $@"
            SELECT 
                CAST(date_trunc('{truncPart}', d.full_date) AS DATE) as ""period_start"",
                AVG(salary_amount) as ""avg_salary"",
                COUNT(*) as ""salary_count_in_period""
            FROM marketstat.fact_salaries fs
            JOIN marketstat.dim_date d ON fs.date_id = d.date_id
            WHERE {whereSql}
            GROUP BY 1
            ORDER BY 1 DESC
            LIMIT {periods}";

        var dtos = await _context.Set<SalaryTimeSeriesPointDto>()
            .FromSqlRaw(sql, sqlParams.ToArray())
            .AsNoTracking()
            .ToListAsync();

        return dtos.Select(d => new SalaryTimeSeriesPoint
        {
            PeriodStart = d.PeriodStart,
            AvgSalary = d.AvgSalary ?? 0,
            SalaryCountInPeriod = d.SalaryCountInPeriod
        }).OrderBy(x => x.PeriodStart).ToList();
    }

    public async Task<IEnumerable<PublicRoleByLocationIndustry>> GetPublicRolesAsync(ResolvedSalaryFilter filter, int minRecordCount)
    {
        var (whereSql, sqlParams) = BuildWhereClause(filter);
        var sql = $@"
            SELECT 
                j.standard_job_role_title as ""standard_job_role_title"",
                AVG(fs.salary_amount) as ""average_salary"",
                COUNT(*) as ""salary_record_count""
            FROM marketstat.fact_salaries fs
            JOIN marketstat.dim_job j ON fs.job_id = j.job_id
            LEFT JOIN marketstat.dim_date d ON fs.date_id = d.date_id
            WHERE {whereSql}
            GROUP BY j.standard_job_role_title
            HAVING COUNT(*) >= {minRecordCount}
            ORDER BY ""average_salary"" DESC";

        var dtos = await _context.Set<PublicRoleByLocationIndustryDto>()
            .FromSqlRaw(sql, sqlParams.ToArray())
            .AsNoTracking()
            .ToListAsync();

        return dtos.Select(d => new PublicRoleByLocationIndustry
        {
            StandardJobRoleTitle = d.StandardJobRoleTitle,
            AverageSalary = d.AverageSalary,
            SalaryRecordCount = d.SalaryRecordCount
        });
    }

    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(ResolvedSalaryFilter filter)
    {
        var (whereSql, sqlParams) = BuildWhereClause(filter);
        
        var sql = $@"
            SELECT 
                fs.salary_fact_id,
                fs.date_id,
                fs.location_id,
                fs.employer_id,
                fs.job_id,
                fs.employee_id,
                fs.salary_amount
            FROM marketstat.fact_salaries fs
            LEFT JOIN marketstat.dim_date d ON fs.date_id = d.date_id
            WHERE {whereSql}
            LIMIT 1000";

        var result = await _context.FactSalaries
            .FromSqlRaw(sql, sqlParams.ToArray())
            .AsNoTracking()
            .ToListAsync();

        return result.Select(FactSalaryConverter.ToDomain);
    }
    
    public Task AddFactSalaryAsync(FactSalary salary) => throw new NotImplementedException();
    public Task<FactSalary> GetFactSalaryByIdAsync(long salaryId) => throw new NotImplementedException();
    public Task UpdateFactSalaryAsync(FactSalary salaryFact) => throw new NotImplementedException();
    public Task DeleteFactSalaryByIdAsync(long salaryFactId) => throw new NotImplementedException();
    
    private (string Sql, List<object> Params) BuildWhereClause(ResolvedSalaryFilter filter)
    {
        var sb = new StringBuilder("1=1");
        var paramsList = new List<object>();
        int idx = 0;

        if (filter.DateStart.HasValue)
        {
            sb.Append($" AND d.full_date >= {{{idx}}}");
            paramsList.Add(filter.DateStart.Value);
            idx++;
        }
        if (filter.DateEnd.HasValue)
        {
            sb.Append($" AND d.full_date <= {{{idx}}}");
            paramsList.Add(filter.DateEnd.Value);
            idx++;
        }
        if (filter.LocationIds != null && filter.LocationIds.Any())
        {
            sb.Append($" AND fs.location_id = ANY({{{idx}}})");
            paramsList.Add(filter.LocationIds.ToArray());
            idx++;
        }
        if (filter.JobIds != null && filter.JobIds.Any())
        {
            sb.Append($" AND fs.job_id = ANY({{{idx}}})");
            paramsList.Add(filter.JobIds.ToArray());
            idx++;
        }

        return (sb.ToString(), paramsList);
    }
}
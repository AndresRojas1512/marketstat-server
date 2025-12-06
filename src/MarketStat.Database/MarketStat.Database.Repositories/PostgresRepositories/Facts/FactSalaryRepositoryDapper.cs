using System.Data;
using System.Text;
using Dapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Responses;
using MarketStat.Common.Enums;
using MarketStat.Database.Core.Repositories.Facts;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Facts;

public class FactSalaryRepositoryDapper : IFactSalaryRepository
{
    private readonly string _connectionString;

    static FactSalaryRepositoryDapper()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    public FactSalaryRepositoryDapper(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    
    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
    
    public async Task<List<SalaryDistributionBucket>> GetSalaryDistributionAsync(ResolvedSalaryFilter filter)
    {
        var parameters = new DynamicParameters();
        var whereSql = BuildWhereClause(filter, parameters);
        
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
                    min_val, max_val, total_count,
                    GREATEST(2, FLOOR(LOG(2.0, GREATEST(total_count, 1))) + 2) as bucket_count
                FROM Stats
            ),
            BucketParams AS (
                SELECT 
                    min_val, max_val, bucket_count, total_count,
                    CASE WHEN bucket_count > 0 THEN (max_val - min_val) / bucket_count ELSE 0 END as width
                FROM Config
            )
            SELECT 
                CAST(min_val + (LEAST(width_bucket(salary_amount, min_val, max_val, CAST(bucket_count AS INT)), CAST(bucket_count AS INT)) - 1) * width AS numeric) as LowerBound,
                CAST(min_val + (LEAST(width_bucket(salary_amount, min_val, max_val, CAST(bucket_count AS INT)), CAST(bucket_count AS INT))) * width AS numeric) as UpperBound,
                COUNT(*) as BucketCount
            FROM RawData
            CROSS JOIN BucketParams
            WHERE total_count > 0 AND width > 0
            GROUP BY 1, 2
            ORDER BY 1";

        using var db = CreateConnection();
        var result = await db.QueryAsync<SalaryDistributionBucket>(sql, parameters);
        return result.ToList();
    }

    public async Task<SalarySummary?> GetSalarySummaryAsync(ResolvedSalaryFilter filter, int targetPercentile)
    {
        var percentileVal = targetPercentile / 100.0;
        var parameters = new DynamicParameters();
        var whereSql = BuildWhereClause(filter, parameters);
        
        var sql = $@"
            SELECT 
                COUNT(*) as TotalCount,
                COALESCE(AVG(salary_amount), 0) as AverageSalary,
                PERCENTILE_CONT(0.25) WITHIN GROUP (ORDER BY salary_amount) as Percentile25,
                PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY salary_amount) as Percentile50,
                PERCENTILE_CONT(0.75) WITHIN GROUP (ORDER BY salary_amount) as Percentile75,
                PERCENTILE_CONT(@TargetP) WITHIN GROUP (ORDER BY salary_amount) as PercentileTarget
            FROM marketstat.fact_salaries fs
            LEFT JOIN marketstat.dim_date d ON fs.date_id = d.date_id
            WHERE {whereSql}";
        parameters.Add("TargetP", percentileVal);
        using var db = CreateConnection();
        return await db.QueryFirstOrDefaultAsync<SalarySummary>(sql, parameters);
    }

    public async Task<List<SalaryTimeSeriesPoint>> GetSalaryTimeSeriesAsync(ResolvedSalaryFilter filter,
        TimeGranularity granularity, int periods)
    {
        var parameters = new DynamicParameters();
        var whereSql = BuildWhereClause(filter, parameters);
        string truncPart = granularity switch
        {
            TimeGranularity.Year => "year",
            TimeGranularity.Quarter => "quarter",
            _ => "month"
        };

        var sql = $@"
            SELECT 
                CAST(date_trunc('{truncPart}', d.full_date) AS DATE) as PeriodStart,
                CAST(AVG(salary_amount) AS numeric) as AvgSalary,
                COUNT(*) as SalaryCountInPeriod
            FROM marketstat.fact_salaries fs
            JOIN marketstat.dim_date d ON fs.date_id = d.date_id
            WHERE {whereSql}
            GROUP BY 1
            ORDER BY 1 DESC
            LIMIT @Periods";
        
        parameters.Add("Periods", periods);
        using var db = CreateConnection();
        var result = await db.QueryAsync<SalaryTimeSeriesPoint>(sql, parameters);
        return result.OrderBy(x => x.PeriodStart).ToList();
    }

    public async Task<IEnumerable<PublicRoleByLocationIndustry>> GetPublicRolesAsync(ResolvedSalaryFilter filter,
        int minRecordCount)
    {
        var parameters = new DynamicParameters();
        var whereSql = BuildWhereClause(filter, parameters);
        var sql = $@"
            SELECT 
                j.standard_job_role_title as StandardJobRoleTitle,
                AVG(fs.salary_amount) as AverageSalary,
                COUNT(*) as SalaryRecordCount
            FROM marketstat.fact_salaries fs
            JOIN marketstat.dim_job j ON fs.job_id = j.job_id
            LEFT JOIN marketstat.dim_date d ON fs.date_id = d.date_id
            WHERE {whereSql}
            GROUP BY j.standard_job_role_title
            HAVING COUNT(*) >= @MinCount
            ORDER BY AverageSalary DESC";
        parameters.Add("MinCount", minRecordCount);
        using var db = CreateConnection();
        return await db.QueryAsync<PublicRoleByLocationIndustry>(sql, parameters);
    }

    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(ResolvedSalaryFilter filter)
    {
        var parameters = new DynamicParameters();
        var whereSql = BuildWhereClause(filter, parameters);
        var sql = $@"
            SELECT 
                fs.salary_fact_id as SalaryFactId,
                fs.date_id as DateId,
                fs.location_id as LocationId,
                fs.employer_id as EmployerId,
                fs.job_id as JobId,
                fs.employee_id as EmployeeId,
                fs.salary_amount as SalaryAmount
            FROM marketstat.fact_salaries fs
            LEFT JOIN marketstat.dim_date d ON fs.date_id = d.date_id
            WHERE {whereSql}
            LIMIT 1000";
        using var db = CreateConnection();
        return await db.QueryAsync<FactSalary>(sql, parameters);
    }
    
    public Task AddFactSalaryAsync(FactSalary salary) => throw new NotImplementedException();
    public Task<FactSalary> GetFactSalaryByIdAsync(long salaryId) => throw new NotImplementedException();
    public Task UpdateFactSalaryAsync(FactSalary salaryFact) => throw new NotImplementedException();
    public Task DeleteFactSalaryByIdAsync(long salaryFactId) => throw new NotImplementedException();

    private string BuildWhereClause(ResolvedSalaryFilter filter, DynamicParameters parameters)
    {
        var sb = new StringBuilder("1=1");
        if (filter.DateStart.HasValue)
        {
            sb.Append(" AND d.full_date >= @DateStart");
            parameters.Add("DateStart", filter.DateStart.Value);
        }
        if (filter.DateEnd.HasValue)
        {
            sb.Append(" AND d.full_date <= @DateEnd");
            parameters.Add("DateEnd", filter.DateEnd.Value);
        }
        if (filter.LocationIds != null && filter.LocationIds.Any())
        {
            sb.Append(" AND fs.location_id = ANY(@LocationIds)");
            parameters.Add("LocationIds", filter.LocationIds.ToArray());
        }
        if (filter.JobIds != null && filter.JobIds.Any())
        {
            sb.Append(" AND fs.job_id = ANY(@JobIds)");
            parameters.Add("JobIds", filter.JobIds.ToArray());
        }
        return sb.ToString();
    }
}

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value;
        parameter.DbType = DbType.Date;
    }

    public override DateOnly Parse(object value)
    {
        return value switch
        {
            DateTime dt => DateOnly.FromDateTime(dt),
            string s => DateOnly.Parse(s),
            _ => (DateOnly)value
        };
    }
}
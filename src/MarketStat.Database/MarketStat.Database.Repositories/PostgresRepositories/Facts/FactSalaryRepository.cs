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

namespace MarketStat.Database.Repositories.PostgresRepositories.Facts;

public class FactSalaryRepository : IFactSalaryRepository
{
    private readonly MarketStatDbContext _dbContext;

    public FactSalaryRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddFactSalaryAsync(FactSalary salary)
    {
        var dbModel = new FactSalaryDbModel(
            salaryFactId: 0,
            dateId: salary.DateId,
            cityId: salary.CityId,
            employerId: salary.EmployerId,
            jobRoleId: salary.JobRoleId,
            employeeId: salary.EmployeeId,
            salaryAmount: salary.SalaryAmount,
            bonusAmount: salary.BonusAmount
        );
        await _dbContext.FactSalaries.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg &&
                  pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException("One or more referenced entities (date, city, employer, job role or employee) were not found.");
        }
        salary.SalaryFactId = dbModel.SalaryFactId;
    }

    public async Task<FactSalary> GetFactSalaryByIdAsync(int salaryId)
    {
        var dbSalary = await _dbContext.FactSalaries.FindAsync(salaryId);
        if (dbSalary is null)
            throw new NotFoundException($"Salary fact with ID {salaryId} not found.");
        return FactSalaryConverter.ToDomain(dbSalary);
    }
    
    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(FactSalaryFilter filter)
    {
        IQueryable<FactSalaryDbModel> q = _dbContext.FactSalaries;

        if (filter.DateId.HasValue) q = q.Where(x => x.DateId == filter.DateId);
        if (filter.CityId.HasValue) q = q.Where(x => x.CityId == filter.CityId);
        if (filter.EmployerId.HasValue) q = q.Where(x => x.EmployerId == filter.EmployerId);
        if (filter.JobRoleId.HasValue) q = q.Where(x => x.JobRoleId == filter.JobRoleId);
        if (filter.EmployeeId.HasValue) q = q.Where(x => x.EmployeeId == filter.EmployeeId);

        var list = await q.ToListAsync();
        return list.Select(FactSalaryConverter.ToDomain);
    }

    public async Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync()
    {
        var all = await _dbContext.FactSalaries.ToListAsync();
        return all.Select(FactSalaryConverter.ToDomain);
    }

    public async Task UpdateFactSalaryAsync(FactSalary salaryFact)
    {
        var dbModel = await _dbContext.FactSalaries.FindAsync(salaryFact.SalaryFactId);
        if (dbModel is null)
            throw new NotFoundException($"Salary fact with ID {salaryFact.SalaryFactId} not found.");

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
            when (dbEx.InnerException is PostgresException pg &&
                  pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException("One or more referenced entities (date, city, employer, job role or employee) were not found.");
        }
    }

    public async Task DeleteFactSalaryByIdAsync(int salaryFactId)
    {
        var dbModel = await _dbContext.FactSalaries.FindAsync(salaryFactId);
        if (dbModel is null)
            throw new NotFoundException($"Salary fact with ID {salaryFactId} not found.");

        _dbContext.FactSalaries.Remove(dbModel);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<SalaryStats> GetSalaryStatsAsync(FactSalaryFilter filter)
    {
        var sql = new StringBuilder("""
                                    SELECT
                                        COUNT(*)                                                        AS count,
                                        MIN(salary_amount)                                              AS min,
                                        MAX(salary_amount)                                              AS max,
                                        AVG(salary_amount)                                              AS mean,
                                        PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY salary_amount)     AS median,
                                        PERCENTILE_CONT(0.25) WITHIN GROUP (ORDER BY salary_amount)     AS p25,
                                        PERCENTILE_CONT(0.75) WITHIN GROUP (ORDER BY salary_amount)     AS p75
                                    FROM   fact_salaries
                                    WHERE  1 = 1
                                    """);

        var parameters = new List<NpgsqlParameter>();
        if (filter.DateId     is { } date)  { sql.Append(" AND date_id     = @date");  parameters.Add(new NpgsqlParameter("date",  date)); }
        if (filter.CityId     is { } city)  { sql.Append(" AND city_id     = @city");  parameters.Add(new NpgsqlParameter("city",  city)); }
        if (filter.EmployerId is { } emp)   { sql.Append(" AND employer_id = @emp");   parameters.Add(new NpgsqlParameter("emp",   emp));  }
        if (filter.JobRoleId  is { } role)  { sql.Append(" AND job_role_id = @role");  parameters.Add(new NpgsqlParameter("role",  role)); }
        if (filter.EmployeeId is { } empl)  { sql.Append(" AND employee_id = @empl");  parameters.Add(new NpgsqlParameter("empl",  empl)); }

        var row = await _dbContext
            .Set<SalaryStatsDbModel>()
            .FromSqlRaw(sql.ToString(), parameters.ToArray())
            .AsNoTracking()
            .SingleAsync();

        return new SalaryStats(
            row.Count,
            row.Min,   row.Max,
            row.Mean,  row.Median,
            row.Percentile25, row.Percentile75);
    }

    public async Task<IReadOnlyList<(DateOnly Date, decimal AvgSalary)>> GetAverageTimeSeriesAsync(
        FactSalaryFilter filter,
        TimeGranularity  granularity)
    {
        IQueryable<FactSalaryDbModel> q = _dbContext.FactSalaries;

        if (filter.CityId.HasValue)     q = q.Where(x => x.CityId     == filter.CityId);
        if (filter.EmployerId.HasValue) q = q.Where(x => x.EmployerId == filter.EmployerId);
        if (filter.JobRoleId.HasValue)  q = q.Where(x => x.JobRoleId  == filter.JobRoleId);
        if (filter.EmployeeId.HasValue) q = q.Where(x => x.EmployeeId == filter.EmployeeId);
        if (filter.DateId.HasValue)     q = q.Where(x => x.DateId     == filter.DateId);

        var joined = from fs in q
            join d in _dbContext.DimDates on fs.DateId equals d.DateId
            select new { d.FullDate, fs.SalaryAmount };

        var grouped = granularity switch
        {
            TimeGranularity.Month =>
                joined.GroupBy(v => new DateOnly(v.FullDate.Year, v.FullDate.Month, 1)),

            TimeGranularity.Quarter =>
                joined.GroupBy(v =>
                    new DateOnly(v.FullDate.Year, (((v.FullDate.Month - 1) / 3) * 3) + 1, 1)),

            _ =>
                joined.GroupBy(v => new DateOnly(v.FullDate.Year, 1, 1))
        };

        var list = await grouped
            .Select(g => new { Date = g.Key, Avg = g.Average(v => v.SalaryAmount) })
            .OrderBy(g => g.Date)
            .ToListAsync();

        return list.Select(e => (e.Date, e.Avg)).ToList();
    }
}
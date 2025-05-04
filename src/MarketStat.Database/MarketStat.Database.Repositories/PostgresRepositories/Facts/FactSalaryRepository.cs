using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;
using Microsoft.EntityFrameworkCore;

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
        await _dbContext.SaveChangesAsync();
        salary.SalaryFactId = dbModel.SalaryFactId;
    }

    public async Task<FactSalary> GetFactSalaryByIdAsync(int salaryId)
    {
        var dbSalary = await _dbContext.FactSalaries.FindAsync(salaryId)
                      ?? throw new KeyNotFoundException($"FactSalary {salaryId} not found.");
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
        var dbModel = await _dbContext.FactSalaries.FindAsync(salaryFact.SalaryFactId)
                      ?? throw new KeyNotFoundException($"Cannot update: salary fact {salaryFact.SalaryFactId} not found.");

        dbModel.DateId       = salaryFact.DateId;
        dbModel.CityId       = salaryFact.CityId;
        dbModel.EmployerId   = salaryFact.EmployerId;
        dbModel.JobRoleId    = salaryFact.JobRoleId;
        dbModel.EmployeeId   = salaryFact.EmployeeId;
        dbModel.SalaryAmount = salaryFact.SalaryAmount;
        dbModel.BonusAmount  = salaryFact.BonusAmount;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteFactSalaryByIdAsync(int salaryFactId)
    {
        var dbModel = await _dbContext.FactSalaries.FindAsync(salaryFactId)
                      ?? throw new KeyNotFoundException($"Cannot delete: salary fact {salaryFactId} not found.");

        _dbContext.FactSalaries.Remove(dbModel);
        await _dbContext.SaveChangesAsync();
    }
}
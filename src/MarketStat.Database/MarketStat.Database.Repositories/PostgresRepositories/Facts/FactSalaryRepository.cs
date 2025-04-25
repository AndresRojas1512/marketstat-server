using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Database.Core.Repositories.Facts;

namespace MarketStat.Database.Repositories.PostgresRepositories.Facts;

public class FactSalaryRepository : IFactSalaryRepository
{
    private readonly Dictionary<int, FactSalary> _salaryFacts = new Dictionary<int, FactSalary>();

    public Task AddFactSalaryAsync(FactSalary salaryFact)
    {
        if (!_salaryFacts.TryAdd(salaryFact.SalaryFactId, salaryFact))
        {
            throw new ArgumentException($"Salary fact {salaryFact.SalaryFactId} already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<FactSalary> GetFactSalaryByIdAsync(int salaryFactId)
    {
        if (_salaryFacts.TryGetValue(salaryFactId, out var salaryFact))
        {
            return Task.FromResult(salaryFact);
        }

        throw new KeyNotFoundException($"Salary fact {salaryFactId} not found.");
    }
    
    public Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(FactSalaryFilter salaryFilter)
    {
        var query = _salaryFacts.Values.AsEnumerable();

        if (salaryFilter.DateId.HasValue)
            query = query.Where(f => f.DateId == salaryFilter.DateId.Value);

        if (salaryFilter.CityId.HasValue)
            query = query.Where(f => f.CityId == salaryFilter.CityId.Value);

        if (salaryFilter.EmployerId.HasValue)
            query = query.Where(f => f.EmployerId == salaryFilter.EmployerId.Value);

        if (salaryFilter.JobRoleId.HasValue)
            query = query.Where(f => f.JobRoleId == salaryFilter.JobRoleId.Value);

        if (salaryFilter.EmployeeId.HasValue)
            query = query.Where(f => f.EmployeeId == salaryFilter.EmployeeId.Value);
        
        return Task.FromResult(query);
    }

    public Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync()
    {
        return Task.FromResult<IEnumerable<FactSalary>>(_salaryFacts.Values);
    }

    public Task UpdateFactSalaryAsync(FactSalary salaryFact)
    {
        if (!_salaryFacts.ContainsKey(salaryFact.SalaryFactId))
        {
            throw new KeyNotFoundException($"Cannot update: salary fact {salaryFact.SalaryFactId} not found.");
        }
        _salaryFacts[salaryFact.SalaryFactId] = salaryFact;
        return Task.CompletedTask;
    }

    public Task DeleteFactSalaryByIdAsync(int salaryFactId)
    {
        if (!_salaryFacts.Remove(salaryFactId))
        {
            throw new KeyNotFoundException($"Cannot delete: salary fact {salaryFactId} not found.");
        }

        return Task.CompletedTask;
    }
}
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEmployerRepository : IDimEmployerRepository
{
    private readonly Dictionary<int, DimEmployer> _employers = new Dictionary<int, DimEmployer>();
    
    public Task AddEmployerAsync(DimEmployer employer)
    {
        if (!_employers.TryAdd(employer.EmployerId, employer))
        {
            throw new ArgumentException($"Employer {employer.EmployerId} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task<DimEmployer> GetEmployerByIdAsync(int employerId)
    {
        if (_employers.TryGetValue(employerId, out var e))
        {
            return Task.FromResult(e);
        }
        throw new KeyNotFoundException($"Employer {employerId} not found.");
    }

    public Task<IEnumerable<DimEmployer>> GetAllEmployersAsync()
    {
        return Task.FromResult<IEnumerable<DimEmployer>>(_employers.Values);
    }

    public Task UpdateEmployerAsync(DimEmployer employer)
    {
        if (!_employers.ContainsKey(employer.EmployerId))
        {
            throw new KeyNotFoundException($"Cannot update: {employer.EmployerId} not found.");
        }
        _employers[employer.EmployerId] = employer;
        return Task.CompletedTask;
    }

    public Task DeleteEmployerAsync(int employerId)
    {
        if (!_employers.ContainsKey(employerId))
        {
            throw new KeyNotFoundException($"Cannot delete: {employerId} not found.");
        }
        return Task.CompletedTask;
    }
}
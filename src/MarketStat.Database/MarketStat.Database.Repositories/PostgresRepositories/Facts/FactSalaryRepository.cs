using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Database.Core.Repositories.Facts;

namespace MarketStat.Database.Repositories.PostgresRepositories.Facts;

public class FactSalaryRepository : IFactSalaryRepository
{
    private readonly List<FactSalary> _store = new();
    
    public Task<IEnumerable<FactSalary>> GetSalaryByFilterAsync(FactSalaryFilter filter)
    {
        var query = _store.AsEnumerable();
        
        if (filter.CityId is int c) query = query.Where(x => x.CityId == c);
        if (filter.EmployerId is int e) query = query.Where(x => x.EmployerId == e);
        if (filter.JobRoleId is int j) query = query.Where(x => x.JobRoleId == j);
        if (filter.DateId is int d) query = query.Where(x => x.DateId == d);
        if (filter.EmployeeId is int p) query = query.Where(x => x.EmployeeId == p);
        
        return Task.FromResult(query);
    }
}
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEmployeeEducationRepository : IDimEmployeeEducationRepository
{
    private readonly Dictionary<(int EmployeeId, int EducationId), DimEmployeeEducation> _links =
        new Dictionary<(int, int), DimEmployeeEducation>();

    public Task AddEmployeeEducationAsync(DimEmployeeEducation link)
    {
        var key = (link.EmployeeId, link.EducationId);
        if (!_links.TryAdd(key, link))
        {
            throw new ArgumentException(
                $"Link already exists for Employee {link.EmployeeId} and Education {link.EducationId}");
        }
        return Task.CompletedTask;
    }

    public Task RemoveEmployeeEducationAsync(int employeeId, int educationId)
    {
        var key = (employeeId, educationId);
        if (!_links.Remove(key))
        {
            throw new KeyNotFoundException(
                $"Cannot remove for Employee {employeeId} and Education {educationId}: not found");
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<DimEmployeeEducation>> GetEducationsByEmployeeIdAsync(int employeeId)
    {
        var results = _links.Values.Where(l => l.EmployeeId == employeeId).ToList();
        return Task.FromResult<IEnumerable<DimEmployeeEducation>>(results);
    }
}
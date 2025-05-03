using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Facts;

public class FactSalaryServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IFactSalaryService _factSalaryService;

    public FactSalaryServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _factSalaryService =
            new FactSalaryService(_accessObject.FactSalaryRepository, NullLogger<FactSalaryService>.Instance);
    }
    
    public void Dispose() => _accessObject.Dispose();

    [Fact]
    public async Task GetAllSalariesAsync_Empty_ReturnsEmpty()
    {
        var all = await _factSalaryService.GetAllFactSalariesAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllFactSalariesAsync_Seeded_ReturnsSeeded()
    {
        var seed = new List<FactSalary>
        {
            new FactSalary(1, dateId:1, cityId:1, employerId:1, jobRoleId:1, employeeId:1, salaryAmount:100m, bonusAmount:10m),
            new FactSalary(2, dateId:2, cityId:2, employerId:2, jobRoleId:2, employeeId:2, salaryAmount:200m, bonusAmount:20m),
        };
        await _accessObject.SeedSalaryAsync(seed);

        var all = (await _factSalaryService.GetAllFactSalariesAsync()).ToList();

        Assert.Equal(2, all.Count);

        Assert.Contains(all, f =>
            f.SalaryFactId == 1
            && f.DateId       == 1
            && f.CityId       == 1
            && f.EmployerId   == 1
            && f.JobRoleId    == 1
            && f.EmployeeId   == 1
            && f.SalaryAmount == 100m
            && f.BonusAmount  == 10m
        );

        Assert.Contains(all, f =>
            f.SalaryFactId == 2
            && f.DateId       == 2
            && f.CityId       == 2
            && f.EmployerId   == 2
            && f.JobRoleId    == 2
            && f.EmployeeId   == 2
            && f.SalaryAmount == 200m
            && f.BonusAmount  == 20m
        );
    }
}
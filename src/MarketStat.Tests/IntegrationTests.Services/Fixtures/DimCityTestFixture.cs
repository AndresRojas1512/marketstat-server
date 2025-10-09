using MarketStat.Database.Context;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Services.Dimensions.DimCityService;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntegrationTests.Services.Fixtures;

public class DimCityTestFixture : IDisposable
{
    public readonly MarketStatDbContext DbContext;
    public readonly DimCityService DimCityService;

    public DimCityTestFixture()
    {
        DbContext = new InMemoryDbContextFactory().GetDbContext();
        var repository = new DimCityRepository(DbContext);
        DimCityService = new DimCityService(repository, NullLogger<DimCityService>.Instance);
        
        SeedInitialData().GetAwaiter().GetResult();
    }

    private async Task SeedInitialData()
    {
        DbContext.DimFederalDistricts.Add(new() { DistrictId = 1, DistrictName = "Central" });
        DbContext.DimOblasts.Add(new() { OblastId = 1, DistrictId = 1, OblastName = "Moscow Oblast" });
        await DbContext.SaveChangesAsync();
    }

    public void Dispose() => DbContext.Dispose();
}
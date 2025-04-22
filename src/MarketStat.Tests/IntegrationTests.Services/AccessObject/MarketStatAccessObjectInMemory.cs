using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntegrationTests.Services.AccessObject;

public class MarketStatAccessObjectInMemory : IDisposable
{
    public MarketStatDbContext Context { get; }
    public IDimEmployerRepository EmployerRepository { get; }
    public IDimEmployerService EmployerService { get; }

    public MarketStatAccessObjectInMemory()
    {
        Context = new InMemoryDbContextFactory().GetDbContext();
        EmployerRepository = new DimEmployerRepository(Context);
        EmployerService = new DimEmployerService(EmployerRepository, NullLogger<DimEmployerService>.Instance);
    }

    public async Task SeedEmployerAsync(IEnumerable<DimEmployer> items)
    {
        foreach (var e in items)
        {
            Context.DimEmployers.Add(DimEmployerConverter.ToDbModel(e));
        }
        await Context.SaveChangesAsync();
    }
    public void Dispose() => Context.Dispose();
}
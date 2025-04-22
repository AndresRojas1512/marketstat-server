using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Context;

public class InMemoryDbContextFactory : IDbContextFactory
{
    private readonly string _dbName;

    public InMemoryDbContextFactory()
    {
        _dbName = "MarketStatTestdb_" + Guid.NewGuid();
    }

    public MarketStatDbContext GetDbContext()
    {
        var builder = new DbContextOptionsBuilder<MarketStatDbContext>().UseInMemoryDatabase(_dbName);
        return new MarketStatDbContext(builder.Options);
    }
}
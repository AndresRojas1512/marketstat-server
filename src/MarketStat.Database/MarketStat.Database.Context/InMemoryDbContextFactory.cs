namespace MarketStat.Database.Context;

using Microsoft.EntityFrameworkCore;

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

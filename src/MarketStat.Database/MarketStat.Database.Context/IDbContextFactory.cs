namespace MarketStat.Database.Context;

public interface IDbContextFactory
{
    MarketStatDbContext GetDbContext();
}

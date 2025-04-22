using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MarketStat.Database.Context;

public class NpgsqlDbContextFactory : IDbContextFactory
{
    private readonly IConfiguration _configuration;

    public NpgsqlDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public MarketStatDbContext GetDbContext()
    {
        var connKey = _configuration["DbConnection"]!;
        var connString = _configuration.GetConnectionString(connKey);

        var builder = new DbContextOptionsBuilder<MarketStatDbContext>();
        builder.UseNpgsql(connString);

        return new MarketStatDbContext(builder.Options);
    }
}
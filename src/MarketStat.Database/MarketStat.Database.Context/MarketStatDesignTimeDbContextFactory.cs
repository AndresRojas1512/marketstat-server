using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MarketStat.Database.Context
{
    public class MarketStatDesignTimeDbContextFactory
        : IDesignTimeDbContextFactory<MarketStatDbContext>
    {
        public MarketStatDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString =
                configuration.GetConnectionString("MarketStatAdmin")
                ?? configuration.GetConnectionString("MarketStat");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "The 'MarketStatAdmin' or fallback 'MarketStat' connection string was not found.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<MarketStatDbContext>();
        
            optionsBuilder.UseNpgsql(connectionString, o =>
            {
                o.MigrationsAssembly(typeof(MarketStatDbContext).Assembly.FullName);
            }).UseSnakeCaseNamingConvention();

            return new MarketStatDbContext(optionsBuilder.Options);
        }
    }
}
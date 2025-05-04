using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MarketStat.Database.Context
{
    public class MarketStatDesignTimeDbContextFactory
        : IDesignTimeDbContextFactory<MarketStatDbContext>
    {
        public MarketStatDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connString = configuration.GetConnectionString("MarketStat")
                             ?? throw new InvalidOperationException("Connection string 'MarketStat' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<MarketStatDbContext>();
            optionsBuilder.UseNpgsql(connString);

            return new MarketStatDbContext(optionsBuilder.Options);
        }
    }
}
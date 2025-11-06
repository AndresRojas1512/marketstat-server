using MarketStat.Database.Context;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Repository.Tests;

public class DatabaseFixture : IDisposable
{
    public readonly DbContextOptions<MarketStatDbContext> Options;

    public DatabaseFixture()
    {
        Options = new DbContextOptionsBuilder<MarketStatDbContext>()
            .UseInMemoryDatabase(databaseName: "MarketStatTestDb")
            .Options;
        using var context = new MarketStatDbContext(Options);
        context.Database.EnsureCreated();

        if (!context.DimEmployers.Any())
        {
            var warmupIndustry = new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "Warmup", IndustryFieldCode = "W.00" };
            context.DimIndustryFields.Add(warmupIndustry);
            context.SaveChanges();
            
            var warmupEmployer = new DimEmployerDbModel
            {
                EmployerName = "Warmup Corp",
                Inn = "0000000000", Ogrn = "0000000000000", Kpp = "000000000",
                RegistrationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                LegalAddress = "Warmup street, 1", ContactEmail = "warmup@example.com",
                ContactPhone = "+7 000 000-00-00", IndustryFieldId = 1
            };
            
            context.DimEmployers.Add(warmupEmployer);
            context.SaveChanges();
            
            var employer = context.DimEmployers.First();
            
            employer.EmployerName = "Warmup Updated";
            context.SaveChanges();
            
            context.DimEmployers.Remove(employer);
            context.DimIndustryFields.Remove(warmupIndustry);
            context.SaveChanges();
        }
    }

    public MarketStatDbContext CreateCleanContext()
    {
        var context = new MarketStatDbContext(Options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        using var context = new MarketStatDbContext(Options);
        context.Database.EnsureDeleted();
    }
}
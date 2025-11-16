using MarketStat.Database.Models;
using MarketStat.Database.Models.MarketStat.Database.Models.Account;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketStat.Database.Context;

public static class DataSeeder
{
    public static async Task SeedDevelopmentDataAsync(MarketStatDbContext dbContext, ILogger logger)
    {
        if (await dbContext.Users.AnyAsync() || await dbContext.FactSalaries.AnyAsync())
        {
            logger.LogInformation("Database already contains data. Skipping seeding.");
            return;
        }
        logger.LogInformation("No data found.Seeding development database...");

        try
        {
            var industry = new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "IT", IndustryFieldCode = "A.01"};
            dbContext.DimIndustryFields.Add(industry);
            
            var location = new DimLocationDbModel { LocationId = 1, CityName = "Moscow", OblastName = "Moscow", DistrictName = "Central" };
            dbContext.DimLocations.Add(location);
            
            var date = new DimDateDbModel { DateId = 1, FullDate = new DateOnly(2024, 1, 1), Year = 2024, Quarter = 1, Month = 1 };
            dbContext.DimDates.Add(date);

            var employee = new DimEmployeeDbModel 
            { 
                EmployeeId = 1, EmployeeRefId = "emp-demo",
                BirthDate = new DateOnly(1990, 1, 1), CareerStartDate = new DateOnly(2015, 1, 1)
            };
            dbContext.DimEmployees.Add(employee);
            
            await dbContext.SaveChangesAsync();

            var job = new DimJobDbModel { JobId = 1, StandardJobRoleTitle = "Engineer", HierarchyLevelName = "Mid", IndustryFieldId = industry.IndustryFieldId };
            dbContext.DimJobs.Add(job);
            
            var employer = new DimEmployerDbModel { EmployerId = 1, EmployerName = "Demo Corp", IndustryFieldId = industry.IndustryFieldId };
            dbContext.DimEmployers.Add(employer);
            
            await dbContext.SaveChangesAsync();

            var adminUser = new UserDbModel
            {
                UserId = 1,
                Username = "admin@demo",
                Email = "admin@demo",
                FullName = "Admin User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin#123"),
                IsAdmin = true,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            dbContext.Users.Add(adminUser);
            
            dbContext.FactSalaries.AddRange(
                new FactSalaryDbModel { DateId = date.DateId, LocationId = location.LocationId, JobId = job.JobId, EmployerId = employer.EmployerId, EmployeeId = employee.EmployeeId, SalaryAmount = 100000 },
                new FactSalaryDbModel { DateId = date.DateId, LocationId = location.LocationId, JobId = job.JobId, EmployerId = employer.EmployerId, EmployeeId = employee.EmployeeId, SalaryAmount = 110000 },
                new FactSalaryDbModel { DateId = date.DateId, LocationId = location.LocationId, JobId = job.JobId, EmployerId = employer.EmployerId, EmployeeId = employee.EmployeeId, SalaryAmount = 120000 }
            );

            await dbContext.SaveChangesAsync();
            logger.LogInformation("Successfully seeded development data.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
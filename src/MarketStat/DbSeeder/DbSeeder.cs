using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;

namespace MarketStat.DbSeeder;

public static class DbSeeder
{
    public static async Task SeedAsync(MarketStatDbContext context)
    {
        if (await context.FactSalaries.AnyAsync())
        {
            Log.Information("Database already contains data. Skipping seed.");
            return;
        }
        Log.Information("Starting Dimension Seeding...");
        
        var industries = new List<DimIndustryFieldDbModel>
        {
            new() { IndustryFieldName = "Tech", IndustryFieldCode = "T.01" },
            new() { IndustryFieldName = "Finance", IndustryFieldCode = "F.02" },
            new() { IndustryFieldName = "Health", IndustryFieldCode = "H.03" },
            new() { IndustryFieldName = "Retail", IndustryFieldCode = "R.04" },
            new() { IndustryFieldName = "Energy", IndustryFieldCode = "E.05" }
        };
        context.DimIndustryFields.AddRange(industries);
        await context.SaveChangesAsync();

        var locations = new List<DimLocationDbModel>();
        locations.Add(new DimLocationDbModel { CityName = "Moscow", OblastName = "Moscow", DistrictName = "Central" });
        locations.Add(new DimLocationDbModel { CityName = "St. Petersburg", OblastName = "Leningrad", DistrictName = "Northwest" });
        var districts = new[] { "Central", "North", "South", "East", "West" };
        foreach (var dist in districts)
        {
            for (int i = 1; i <= 5; i++)
            {
                var oblast = $"{dist}-Oblast-{i}";
                for (int j = 1; j <= 4; j++)
                {
                    locations.Add(new DimLocationDbModel
                    {
                        DistrictName = dist,
                        OblastName = oblast,
                        CityName = $"{oblast}-City-{j}"
                    });
                }
            }
        }
        context.DimLocations.AddRange(locations);

        var dates = new List<DimDateDbModel>();
        int dateIdCounter = 1;
        for (int y = 2020; y <= 2024; y++)
        {
            for (int m = 1; m <= 12; m++)
            {
                dates.Add(new DimDateDbModel
                {
                    DateId = dateIdCounter++,
                    FullDate = new DateOnly(y, m, 1),
                    Year = y,
                    Quarter = (m - 1) / 3 + 1,
                    Month = m
                });
            }
        }
        context.DimDates.AddRange(dates);

        var jobs = new List<DimJobDbModel>();
        var levels = new[] { "Intern", "Junior", "Mid", "Senior", "Lead" };
        var titles = new[]
            { "Developer", "Analyst", "Manager", "HR", "Sales", "Marketing", "Support", "DevOps", "QA", "Product" };
        var rand = new Random(42);
        foreach (var title in titles)
        {
            foreach (var level in levels)
            {
                jobs.Add(new DimJobDbModel
                {
                    JobRoleTitle = $"{level} {title}",
                    StandardJobRoleTitle = title,
                    HierarchyLevelName = level,
                    IndustryFieldId = industries[rand.Next(industries.Count)].IndustryFieldId
                });
            }
        }
        context.DimJobs.AddRange(jobs);

        var employer = new DimEmployerDbModel
        {
            EmployerName = "MegaCorp",
            Inn = "1234567890",
            Ogrn = "1234567890123",
            Kpp = "123456789",
            RegistrationDate = new DateOnly(2020, 1, 1),
            LegalAddress = "HQ",
            ContactEmail = "admin@corp.com",
            ContactPhone = "111-1111",
            IndustryFieldId = industries[0].IndustryFieldId
        };
        context.DimEmployers.Add(employer);

        var education = new DimEducationDbModel
            { SpecialtyName = "CS", SpecialtyCode = "01", EducationLevelName = "BSc" };
        context.DimEducations.Add(education);
        
        var employee = new DimEmployeeDbModel
        {
            EmployeeRefId = "emp_bench",
            BirthDate = new DateOnly(1990, 1, 1),
            CareerStartDate = new DateOnly(2012, 1, 1),
            Education = education
        };
        context.DimEmployees.Add(employee);
        await context.SaveChangesAsync();

        var connString = context.Database.GetConnectionString();
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        using var writer = await conn.BeginBinaryImportAsync(
            "COPY marketstat.fact_salaries (date_id, location_id, employer_id, job_id, employee_id, salary_amount) FROM STDIN (FORMAT BINARY)");
        int dateCount = dates.Count;
        int locCount = locations.Count;
        int jobCount = jobs.Count;
        int empId = employee.EmployeeId;
        int emplId = employer.EmployerId;

        for (int i = 0; i < 100000; i++)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(dates[rand.Next(dateCount)].DateId);
            await writer.WriteAsync(locations[rand.Next(locCount)].LocationId);
            await writer.WriteAsync(emplId);
            await writer.WriteAsync(jobs[rand.Next(jobCount)].JobId);
            await writer.WriteAsync(empId);
            await writer.WriteAsync((decimal)rand.Next(30000, 500000));
        }
        await writer.CompleteAsync();
    }
}
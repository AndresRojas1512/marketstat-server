using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Responses;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;
using MarketStat.Database.Context;
using MarketStat.Database.Models;
using MarketStat.Database.Models.MarketStat.Database.Models.Account;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;
using MarketStat.Integration.Tests;

namespace MarketStat.Tests.E2E;

[Collection("Integration")]
public class AnalyticsE2E : IAsyncLifetime
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    
    private readonly IntegrationTestFixture _fixture;
    private readonly MarketStatDbContext _dbContext;

    public AnalyticsE2E(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _dbContext = _fixture.CreateContext();
        _http = new HttpClient();
        _baseUrl = Environment.GetEnvironmentVariable("E2E_BASE_URL")
                   ?? "http://localhost:8080";
    }
    
    public async Task InitializeAsync()
    {
        // 1. Clean the database from the previous integration test run
        await _fixture.ResetDatabaseAsync();

        // 2. Seed *only* the admin user needed for this test
        // NOTE: Use the correct password hash from your API's DataSeeder
        _dbContext.Users.Add(new UserDbModel
        {
            UserId = 1,
            Username = "admin@demo",
            Email = "admin@demo.com",
            FullName = "Test Admin",
            IsAdmin = true,
            IsActive = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin#123")
        });
        
        // 3. Seed the salary data that the test assertions are checking for
        // (This data was also being wiped by the reset)
        _dbContext.DimDates.Add(new DimDateDbModel { DateId = 1, FullDate = new DateOnly(2024, 1, 1), Year = 2024, Quarter = 1, Month = 1 });
        _dbContext.DimLocations.Add(new DimLocationDbModel { LocationId = 1, CityName = "Moscow", OblastName = "Moscow", DistrictName = "Central" });
        _dbContext.DimIndustryFields.Add(new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "IT", IndustryFieldCode = "A.01"});
        _dbContext.DimEmployees.Add(new DimEmployeeDbModel { EmployeeId = 1, EmployeeRefId = "emp-1", BirthDate = new DateOnly(1990, 1, 1), CareerStartDate = new DateOnly(2015, 1, 1) });
        await _dbContext.SaveChangesAsync();
        
        _dbContext.DimJobs.Add(new DimJobDbModel { JobId = 1, StandardJobRoleTitle = "Engineer", HierarchyLevelName = "Mid", IndustryFieldId = 1 });
        _dbContext.DimEmployers.Add(new DimEmployerDbModel { EmployerId = 1, EmployerName = "Tech Corp", IndustryFieldId = 1 });
        await _dbContext.SaveChangesAsync();
        
        _dbContext.FactSalaries.AddRange(
            new FactSalaryDbModel { SalaryFactId = 1, DateId = 1, LocationId = 1, JobId = 1, EmployerId = 1, EmployeeId = 1, SalaryAmount = 100000 },
            new FactSalaryDbModel { SalaryFactId = 2, DateId = 1, LocationId = 1, JobId = 1, EmployerId = 1, EmployeeId = 1, SalaryAmount = 110000 },
            new FactSalaryDbModel { SalaryFactId = 3, DateId = 1, LocationId = 1, JobId = 1, EmployerId = 1, EmployeeId = 1, SalaryAmount = 120000 }
        );
        await _dbContext.SaveChangesAsync();
    }
    
    public Task DisposeAsync()
    {
        // This will clean up after the E2E test finishes
        return _fixture.ResetDatabaseAsync();
    }

    [Fact(DisplayName = "MVP flow: login -> analytics endpoints succeed")]
    public async Task AnalyticsFlow_Succeeds()
    {
        var token = await LoginAsync("admin@demo", "Admin#123");
        _http.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        var summary = await GetAsync<SalarySummary>(
            $"{_baseUrl}/api/factsalary/summary?TargetPercentile=50"
        );
        summary.Should().NotBeNull();
        summary.TotalCount.Should().Be(3);
        summary.AverageSalary.Should().Be(110000);
        summary.PercentileTarget.Should().Be(110000);

        var distribution = await GetAsync<List<SalaryDistributionBucketDto>>(
            $"{_baseUrl}/api/factsalary/distribution"
        );
        distribution.Should().NotBeNull();
        distribution.Should().NotBeEmpty();
        distribution.Sum(b => b.BucketCount).Should().Be(3);

        var timeSeries = await GetAsync<List<SalaryTimeSeriesPointDto>>(
            $"{_baseUrl}/api/factsalary/timeseries?Granularity=Year&Periods=1&DateEnd=2024-12-31"
        );
        timeSeries.Should().NotBeNull();
        timeSeries.Should().HaveCount(1);
        timeSeries[0].PeriodStart.Should().Be(new DateOnly(2024, 1, 1));
        timeSeries[0].SalaryCountInPeriod.Should().Be(3);
        timeSeries[0].AvgSalary.Should().Be(110000);
    }

    private async Task<string> LoginAsync(string username, string password)
    {
        var loginRequest = new LoginRequestDto
        {
            Username = username,
            Password = password
        };

        var response = await _http.PostAsJsonAsync($"{_baseUrl}/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        return authResponse.Token;
    }

    private async Task<T> GetAsync<T>(string url)
    {
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<T>();
        content.Should().NotBeNull();
        return content!;
    }
}
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;
using MarketStat.Database.Context;
using MarketStat.Tests.TestData.Builders.Dimensions;
using MarketStat.Tests.TestData.Builders.Facts;
using Microsoft.Extensions.DependencyInjection;

namespace MarketStat.Tests.E2E;

[Collection("E2E")]
public class AnalyticsE2E : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly Func<Task> _resetDatabase;
    private readonly IServiceScopeFactory _scopeFactory;

    public AnalyticsE2E(MarketStatE2ETestWebAppFactory factory)
    {
        _resetDatabase = factory.ResetDatabaseAsync;
        if (factory.KestrelHost == null)
        {
            try
            {
                using var _ = factory.CreateClient();
            }
            catch (InvalidCastException)
            {
            }
        }

        _scopeFactory = factory.KestrelHost!.Services.GetRequiredService<IServiceScopeFactory>();
        _client = factory.CreateRealHttpClient();
    }

    public Task InitializeAsync() => _resetDatabase();

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetPublicRoles_WithMixedData_ReturnsOnlyRolesAboveThresholdAndOrderedBySalary()
    {
        // ARRANGE: Seed ONLY Facts
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MarketStatDbContext>();

            // NOTE: We assume IDs 1, 2, 3 for Jobs/Dates/etc. exist from the Factory Seed
            var facts = new List<MarketStat.Common.Core.MarketStat.Common.Core.Facts.FactSalary>();

            // 15 records for Job 1 (Senior Architect)
            for (int i = 0; i < 15; i++)
            {
                facts.Add(new FactSalaryBuilder()
                    .WithJobId(1)
                    .WithSalaryAmount(200000) 
                    .WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1)
                    .Build());
            }

            // 12 records for Job 2 (Junior Support)
            for (int i = 0; i < 12; i++)
            {
                facts.Add(new FactSalaryBuilder()
                    .WithJobId(2)
                    .WithSalaryAmount(50000)
                    .WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1)
                    .Build());
            }

            // 5 records for Job 3 (Rare Specialist)
            for (int i = 0; i < 5; i++)
            {
                facts.Add(new FactSalaryBuilder()
                    .WithJobId(3)
                    .WithSalaryAmount(120000)
                    .WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1)
                    .Build());
            }

            dbContext.FactSalaries.AddRange(facts.Select(f => FactSalaryConverter.ToDbModel(f)));
            await dbContext.SaveChangesAsync();
        }

        // ACT
        var response = await _client.GetAsync("/api/factsalaries/public/roles?minRecordCount=10");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<PublicRoleByLocationIndustryDto>>();
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);

        result![0].StandardJobRoleTitle.Should().Be("Senior Architect");
        result[0].AverageSalary.Should().Be(200000);
        result[0].SalaryRecordCount.Should().Be(15);

        result[1].StandardJobRoleTitle.Should().Be("Junior Support");
        result[1].AverageSalary.Should().Be(50000);
        
        result.Should().NotContain(x => x.StandardJobRoleTitle == "Rare Specialist");
    }
}

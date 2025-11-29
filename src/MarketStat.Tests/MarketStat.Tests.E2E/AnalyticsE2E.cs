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
            using var _ = factory.CreateDefaultClient(); 
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
        // ARRANGE: Seed Data
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MarketStatDbContext>();

            // 1. Create shared dimensions
            var date = new DimDateBuilder().WithId(1).Build();
            var location = new DimLocationBuilder().WithId(1).Build();
            var industry = new DimIndustryFieldBuilder().WithId(1).Build();
            
            var education = new DimEducationBuilder().WithId(1).Build(); 

            var employer = new DimEmployerBuilder().WithId(1).WithIndustryFieldId(industry.IndustryFieldId).Build();
            
            // Verify the builder uses the correct EducationId
            var employee = new DimEmployeeBuilder().WithId(1).WithEducationId(education.EducationId).Build();

            dbContext.Add(DimDateConverter.ToDbModel(date));
            dbContext.Add(DimLocationConverter.ToDbModel(location));
            dbContext.Add(DimIndustryFieldConverter.ToDbModel(industry));
            
            dbContext.Add(DimEducationConverter.ToDbModel(education)); 

            dbContext.Add(DimEmployerConverter.ToDbModel(employer));
            dbContext.Add(DimEmployeeConverter.ToDbModel(employee));

            // 2. Create 3 distinct Jobs
            var jobHigh = new DimJobBuilder().WithId(1).WithStandardJobRoleTitle("Senior Architect").WithIndustryFieldId(industry.IndustryFieldId).Build();
            var jobLow = new DimJobBuilder().WithId(2).WithStandardJobRoleTitle("Junior Support").WithIndustryFieldId(industry.IndustryFieldId).Build();
            var jobRare = new DimJobBuilder().WithId(3).WithStandardJobRoleTitle("Rare Specialist").WithIndustryFieldId(industry.IndustryFieldId).Build();

            dbContext.DimJobs.AddRange(
                DimJobConverter.ToDbModel(jobHigh),
                DimJobConverter.ToDbModel(jobLow),
                DimJobConverter.ToDbModel(jobRare)
            );
            
            await dbContext.SaveChangesAsync();

            // 3. Generate Facts
            var facts = new List<MarketStat.Common.Core.MarketStat.Common.Core.Facts.FactSalary>();

            // Add 15 records for High Job
            for (int i = 0; i < 15; i++)
            {
                facts.Add(new FactSalaryBuilder()
                    .WithJobId(jobHigh.JobId)
                    .WithSalaryAmount(200000) 
                    .WithDateId(date.DateId).WithLocationId(location.LocationId).WithEmployerId(employer.EmployerId).WithEmployeeId(employee.EmployeeId)
                    .Build());
            }

            // Add 12 records for Low Job
            for (int i = 0; i < 12; i++)
            {
                facts.Add(new FactSalaryBuilder()
                    .WithJobId(jobLow.JobId)
                    .WithSalaryAmount(50000)
                    .WithDateId(date.DateId).WithLocationId(location.LocationId).WithEmployerId(employer.EmployerId).WithEmployeeId(employee.EmployeeId)
                    .Build());
            }

            // Add 5 records for Rare Job
            for (int i = 0; i < 5; i++)
            {
                facts.Add(new FactSalaryBuilder()
                    .WithJobId(jobRare.JobId)
                    .WithSalaryAmount(120000)
                    .WithDateId(date.DateId).WithLocationId(location.LocationId).WithEmployerId(employer.EmployerId).WithEmployeeId(employee.EmployeeId)
                    .Build());
            }

            var dbFacts = facts.Select(f => FactSalaryConverter.ToDbModel(f));
            dbContext.FactSalaries.AddRange(dbFacts);
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
        result[1].SalaryRecordCount.Should().Be(12);
        
        result.Should().NotContain(x => x.StandardJobRoleTitle == "Rare Specialist");
    }
}

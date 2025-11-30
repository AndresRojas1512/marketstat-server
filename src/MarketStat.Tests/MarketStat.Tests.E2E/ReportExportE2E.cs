using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;
using MarketStat.Database.Context;
using MarketStat.Tests.TestData.Builders.Facts;
using Microsoft.Extensions.DependencyInjection;

namespace MarketStat.Tests.E2E;

[Collection("E2E")]
public class ReportExportE2E : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly Func<Task> _resetDatabase;
    private readonly IServiceScopeFactory _scopeFactory;

    public ReportExportE2E(MarketStatE2ETestWebAppFactory factory)
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
    public async Task ExportSalarySummary_ShouldUploadToS3AndReturnUrl()
    {
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MarketStatDbContext>();
            var fact = new FactSalaryBuilder()
                .WithSalaryAmount(50000)
                .WithJobId(1).WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1)
                .Build();
            dbContext.FactSalaries.Add(FactSalaryConverter.ToDbModel(fact));
            await dbContext.SaveChangesAsync();
        }

        var requestDto = new SalarySummaryRequestDto
        {
            CityName = "Moscow",
            TargetPercentile = 50
        };
        var response = await _client.PostAsJsonAsync("/api/reports/salary-summary/export", requestDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var url = doc.RootElement.GetProperty("url").GetString();
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("marketstat-reports");
        url.Should().Contain(".json");
        url.Should().StartWith("http://wiremock:8080");
    }
}
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Responses;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;

namespace MarketStat.Tests.E2E;

public class AnalyticsE2E
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public AnalyticsE2E()
    {
        _http = new HttpClient();
        _baseUrl = Environment.GetEnvironmentVariable("E2E_BASE_URL")
                   ?? "http://localhost:8080";
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
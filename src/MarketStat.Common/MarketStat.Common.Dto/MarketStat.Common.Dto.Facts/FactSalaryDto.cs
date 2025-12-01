namespace MarketStat.Common.Dto.Facts;

using System.Text.Json.Serialization;

public record FactSalaryDto
{
    [JsonPropertyName("salaryFactId")]
    public long SalaryFactId { get; init; }

    [JsonPropertyName("dateId")]
    public int DateId { get; init; }

    [JsonPropertyName("locationId")]
    public int LocationId { get; init; }

    [JsonPropertyName("employerId")]
    public int EmployerId { get; init; }

    [JsonPropertyName("jobId")]
    public int JobId { get; init; }

    [JsonPropertyName("employeeId")]
    public int EmployeeId { get; init; }

    [JsonPropertyName("salaryAmount")]
    public decimal SalaryAmount { get; init; }
}

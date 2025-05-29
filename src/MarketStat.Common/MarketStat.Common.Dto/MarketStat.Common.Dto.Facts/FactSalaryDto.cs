using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public record FactSalaryDto
{
    [JsonPropertyName("salaryFactId")]
    public long SalaryFactId { get; init; }

    [JsonPropertyName("dateId")]
    public int DateId { get; init; }

    [JsonPropertyName("cityId")]
    public int CityId { get; init; }

    [JsonPropertyName("employerId")]
    public int EmployerId { get; init; }

    [JsonPropertyName("jobRoleId")]
    public int JobRoleId { get; init; }

    [JsonPropertyName("employeeId")]
    public int EmployeeId { get; init; }

    [JsonPropertyName("salaryAmount")]
    public decimal SalaryAmount { get; init; }

    [JsonPropertyName("bonusAmount")]
    public decimal BonusAmount { get; init; }
}
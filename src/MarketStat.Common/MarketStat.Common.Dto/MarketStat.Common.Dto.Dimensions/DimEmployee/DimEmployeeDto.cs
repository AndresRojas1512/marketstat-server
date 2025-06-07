using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployee;

public record DimEmployeeDto
{
    [JsonPropertyName("employeeId")]
    public int EmployeeId { get; init; }

    [JsonPropertyName("employeeRefId")]
    public string EmployeeRefId { get; init; } = string.Empty;

    [JsonPropertyName("birthDate")]
    public DateOnly BirthDate { get; init; }

    [JsonPropertyName("careerStartDate")]
    public DateOnly CareerStartDate { get; init; }

    [JsonPropertyName("gender")]
    public string? Gender { get; init; }
}
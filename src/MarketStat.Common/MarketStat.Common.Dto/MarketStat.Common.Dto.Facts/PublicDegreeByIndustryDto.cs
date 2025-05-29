using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class PublicDegreeByIndustryDto
{
    [JsonPropertyName("educationSpecialty")]
    public string? EducationSpecialty { get; set; }

    [JsonPropertyName("employeeWithDegreeCount")]
    public long EmployeeWithDegreeCount { get; set; }
}
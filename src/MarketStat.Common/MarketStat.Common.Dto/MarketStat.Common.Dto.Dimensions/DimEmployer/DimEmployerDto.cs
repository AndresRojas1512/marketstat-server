using System.Text.Json.Serialization;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;

public class DimEmployerDto
{
    [JsonPropertyName("employerId")]
    public int EmployerId { get; init; }

    [JsonPropertyName("employerName")]
    public string EmployerName { get; init; } = string.Empty;

    [JsonPropertyName("inn")]
    public string Inn { get; init; } = string.Empty;

    [JsonPropertyName("ogrn")]
    public string Ogrn { get; init; } = string.Empty;

    [JsonPropertyName("kpp")]
    public string Kpp { get; init; } = string.Empty;

    [JsonPropertyName("registrationDate")]
    public DateOnly RegistrationDate { get; init; }

    [JsonPropertyName("legalAddress")]
    public string LegalAddress { get; init; } = string.Empty;

    [JsonPropertyName("contactEmail")]
    public string ContactEmail { get; init; } = string.Empty;

    [JsonPropertyName("contactPhone")]
    public string ContactPhone { get; init; } = string.Empty;
    
    [JsonPropertyName("industryFieldId")]
    public int IndustryFieldId { get; init; }
    
    [JsonPropertyName("industryField")]
    public DimIndustryFieldDto? IndustryField { get; init; }
}

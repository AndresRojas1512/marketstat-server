using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class PublicSalaryByEducationInIndustryDto
{
    public string EducationSpecialty { get; set; } = string.Empty;
    public string EducationLevelName { get; set; } = string.Empty;
    public decimal AverageSalary { get; set; }
    public long EmployeeCountForLevel { get; set; }
    public long OverallSpecialtyEmployeeCount { get; set; }
}
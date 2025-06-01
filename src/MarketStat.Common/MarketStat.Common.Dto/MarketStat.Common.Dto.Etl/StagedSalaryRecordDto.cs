namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Etl;

public class StagedSalaryRecordDto
{
    public string? RecordedDateText { get; set; }
    public string? CityName { get; set; }
    public string? OblastName { get; set; }
    public string? EmployerName { get; set; }
    public string? StandardJobRoleTitle { get; set; }
    public string? JobRoleTitle { get; set; }
    public string? HierarchyLevelName { get; set; }
    public string? EmployeeBirthDateText { get; set; }
    public string? EmployeeCareerStartDateText { get; set; }
    public decimal? SalaryAmount { get; set; }
    public decimal? BonusAmount { get; set; }
}
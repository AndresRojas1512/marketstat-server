namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class PublicTopEmployerRoleSalariesInIndustryDto
{
    public string EmployerName { get; set; } = string.Empty;
    public string StandardJobRoleTitle { get; set; } = string.Empty;
    public decimal AverageSalaryForRole { get; set; }
    public long SalaryRecordCountForRole { get; set; }
    public long EmployerRank { get; set; }
    public long RoleRankWithinEmployer { get; set; }
}
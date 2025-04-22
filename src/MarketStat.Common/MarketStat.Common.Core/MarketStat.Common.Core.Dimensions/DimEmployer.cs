namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEmployer
{
    public int EmployerId { get; set; }
    public string EmployerName { get; set; }
    public string Industry { get; set; }
    public bool IsPublic { get; set; }

    public DimEmployer(int employerId, string employerName, string industry, bool isPublic)
    {
        EmployerId = employerId;
        EmployerName = employerName;
        Industry = industry;
        IsPublic = isPublic;
    }
}
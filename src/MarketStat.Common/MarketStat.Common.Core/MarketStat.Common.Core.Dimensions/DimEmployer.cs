namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEmployer
{
    public int EmployerId { get; set; }
    public string EmployerName { get; set; }
    public bool IsPublic { get; set; }

    public DimEmployer(int employerId, string employerName, bool isPublic)
    {
        EmployerId = employerId;
        EmployerName = employerName;
        IsPublic = isPublic;
    }
}
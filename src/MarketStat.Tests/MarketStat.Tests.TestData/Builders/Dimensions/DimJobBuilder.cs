namespace MarketStat.Tests.TestData.Builders.Dimensions;

using MarketStat.Common.Core.Dimensions;

public class DimJobBuilder
{
    private int _jobId;
    private string _jobRoleTitle = "Software Developer";
    private string _standardJobRoleTitle = "Software Engineer";
    private string _hierarchyLevelName = "Mid-Level";
    private int _industryFieldId = 1;

    public DimJobBuilder WithId(int id)
    {
        _jobId = id;
        return this;
    }

    public DimJobBuilder WithJobRoleTitle(string title)
    {
        _jobRoleTitle = title;
        return this;
    }

    public DimJobBuilder WithStandardJobRoleTitle(string title)
    {
        _standardJobRoleTitle = title;
        return this;
    }

    public DimJobBuilder WithHierarchyLevelName(string levelName)
    {
        _hierarchyLevelName = levelName;
        return this;
    }

    public DimJobBuilder WithIndustryFieldId(int industryFieldId)
    {
        _industryFieldId = industryFieldId;
        return this;
    }

    public DimJob Build()
    {
        return new DimJob(
            _jobId,
            _jobRoleTitle,
            _standardJobRoleTitle,
            _hierarchyLevelName,
            _industryFieldId);
    }
}

using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimJobConverter
{
    public static DimJobDbModel ToDbModel(DimJob domain)
    {
        return new DimJobDbModel
        {
            JobId = domain.JobId,
            JobRoleTitle = domain.JobRoleTitle,
            StandardJobRoleTitle = domain.StandardJobRoleTitle,
            HierarchyLevelName = domain.HierarchyLevelName,
            IndustryFieldId = domain.IndustryFieldId
        };
    }

    public static DimJob ToDomain(DimJobDbModel dbModel)
    {
        return new DimJob
        {
            JobId = dbModel.JobId,
            JobRoleTitle = dbModel.JobRoleTitle,
            StandardJobRoleTitle = dbModel.StandardJobRoleTitle,
            HierarchyLevelName = dbModel.HierarchyLevelName,
            IndustryFieldId = dbModel.IndustryFieldId,
            IndustryField = dbModel.IndustryField != null
                ? DimIndustryFieldConverter.ToDomain(dbModel.IndustryField)
                : null
        };
    }
}
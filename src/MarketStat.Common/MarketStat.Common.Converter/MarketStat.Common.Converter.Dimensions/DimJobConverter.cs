using MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.Dimensions;

public static class DimJobConverter
{
    public static DimJobDbModel ToDbModel(DimJob domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new DimJobDbModel
        {
            JobId = domain.JobId,
            JobRoleTitle = domain.JobRoleTitle,
            StandardJobRoleTitle = domain.StandardJobRoleTitle,
            HierarchyLevelName = domain.HierarchyLevelName,
            IndustryFieldId = domain.IndustryFieldId,
        };
    }

    public static DimJob ToDomain(DimJobDbModel dbModel)
    {
        ArgumentNullException.ThrowIfNull(dbModel);

        return new DimJob
        {
            JobId = dbModel.JobId,
            JobRoleTitle = dbModel.JobRoleTitle,
            StandardJobRoleTitle = dbModel.StandardJobRoleTitle,
            HierarchyLevelName = dbModel.HierarchyLevelName,
            IndustryFieldId = dbModel.IndustryFieldId,
            IndustryField = dbModel.IndustryField != null
                ? DimIndustryFieldConverter.ToDomain(dbModel.IndustryField)
                : null,
        };
    }
}

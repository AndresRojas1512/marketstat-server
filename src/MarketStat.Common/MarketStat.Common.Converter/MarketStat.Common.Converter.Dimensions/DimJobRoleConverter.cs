using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimJobRoleConverter
{
    public static DimJobRoleDbModel ToDbModel(DimJobRole dimJobRole)
    {
        return new DimJobRoleDbModel(
            dimJobRole.JobRoleId,
            dimJobRole.JobRoleTitle,
            dimJobRole.StandardJobRoleId,
            dimJobRole.HierarchyLevelId
        );
    }

    public static DimJobRole ToDomain(DimJobRoleDbModel dbJobRole)
    {
        return new DimJobRole(
            dbJobRole.JobRoleId,
            dbJobRole.JobRoleTitle,
            dbJobRole.StandardJobRoleId,
            dbJobRole.HierarchyLevelId
        );
    }
}
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public static class DimStandardJobRoleConverter
{
    public static DimStandardJobRoleDbModel ToDbModel(DimStandardJobRole domainJobRole)
    {
        if (domainJobRole == null)
            throw new ArgumentNullException(nameof(domainJobRole));

        return new DimStandardJobRoleDbModel
        {
            StandardJobRoleId = domainJobRole.StandardJobRoleId,
            StandardJobRoleCode = domainJobRole.StandardJobRoleCode,
            StandardJobRoleTitle = domainJobRole.StandardJobRoleTitle,
            IndustryFieldId = domainJobRole.IndustryFieldId
        };
    }

    public static DimStandardJobRole ToDomain(DimStandardJobRoleDbModel dbJobRole)
    {
        if (dbJobRole == null)
            throw new ArgumentNullException(nameof(dbJobRole));

        return new DimStandardJobRole
        {
            StandardJobRoleId = dbJobRole.StandardJobRoleId,
            StandardJobRoleCode = dbJobRole.StandardJobRoleCode,
            StandardJobRoleTitle = dbJobRole.StandardJobRoleTitle,
            IndustryFieldId = dbJobRole.IndustryFieldId
        };
    }
}
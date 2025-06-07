using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public static class DimIndustryFieldConverter
{
    public static DimIndustryFieldDbModel ToDbModel(DimIndustryField domainIndustryField)
    {
        if (domainIndustryField == null)
            throw new ArgumentNullException(nameof(domainIndustryField));

        return new DimIndustryFieldDbModel
        {
            IndustryFieldId = domainIndustryField.IndustryFieldId,
            IndustryFieldCode = domainIndustryField.IndustryFieldCode,
            IndustryFieldName = domainIndustryField.IndustryFieldName
        };
    }

    public static DimIndustryField ToDomain(DimIndustryFieldDbModel dbIndustryField)
    {
        if (dbIndustryField == null)
            throw new ArgumentNullException(nameof(dbIndustryField));

        return new DimIndustryField
        {
            IndustryFieldId = dbIndustryField.IndustryFieldId,
            IndustryFieldCode = dbIndustryField.IndustryFieldCode,
            IndustryFieldName = dbIndustryField.IndustryFieldName
        };
    }
}
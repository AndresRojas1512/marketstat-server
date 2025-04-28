using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimIndustryFieldConverter
{
    public static DimIndustryFieldDbModel ToDbModel(DimIndustryField dimIndustryField)
    {
        return new DimIndustryFieldDbModel(
            dimIndustryField.IndustryFieldId,
            dimIndustryField.IndustryFieldName
        );
    }

    public static DimIndustryField ToDomain(DimIndustryFieldDbModel dbIndustryField)
    {
        return new DimIndustryField(
            dbIndustryField.IndustryFieldId,
            dbIndustryField.IndustryFieldName
        );
    }
}
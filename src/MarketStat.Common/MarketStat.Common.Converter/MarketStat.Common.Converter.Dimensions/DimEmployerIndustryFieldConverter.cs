using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimEmployerIndustryFieldConverter
{
    public static DimEmployerIndustryFieldDbModel ToDbModel(DimEmployerIndustryField domain)
    {
        return new DimEmployerIndustryFieldDbModel(
            domain.EmployerId,
            domain.IndustryFieldId
        );
    }

    public static DimEmployerIndustryField ToDomain(DimEmployerIndustryFieldDbModel dbModel)
    {
        return new DimEmployerIndustryField(
            dbModel.EmployerId,
            dbModel.IndustryFieldId
        );
    }
}
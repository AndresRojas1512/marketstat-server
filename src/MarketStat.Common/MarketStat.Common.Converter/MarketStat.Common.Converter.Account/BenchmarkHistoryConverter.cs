using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Database.Models.MarketStat.Database.Models.Account;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Account;

public static class BenchmarkHistoryConverter
{
    public static BenchmarkHistoryDbModel ToDbModel(BenchmarkHistory domainHistory)
    {
        if (domainHistory == null)
            throw new ArgumentNullException(nameof(domainHistory));

        var dbModel = new BenchmarkHistoryDbModel
        {
            BenchmarkHistoryId = domainHistory.BenchmarkHistoryId,
            UserId = domainHistory.UserId,
            BenchmarkName = domainHistory.BenchmarkName,
            SavedAt = domainHistory.SavedAt,

            FilterIndustryFieldId = domainHistory.FilterIndustryFieldId,
            FilterStandardJobRoleId = domainHistory.FilterStandardJobRoleId,
            FilterHierarchyLevelId = domainHistory.FilterHierarchyLevelId,
            FilterDistrictId = domainHistory.FilterDistrictId,
            FilterOblastId = domainHistory.FilterOblastId,
            FilterCityId = domainHistory.FilterCityId,
            FilterDateStart = domainHistory.FilterDateStart,
            FilterDateEnd = domainHistory.FilterDateEnd,
            FilterTargetPercentile = domainHistory.FilterTargetPercentile,
            FilterGranularity = domainHistory.FilterGranularity,
            FilterPeriods = domainHistory.FilterPeriods,

            BenchmarkResultJson = domainHistory.BenchmarkResultJson
        };
        return dbModel;
    }
    
    public static BenchmarkHistory ToDomain(BenchmarkHistoryDbModel dbHistory)
    {
        if (dbHistory == null)
            throw new ArgumentNullException(nameof(dbHistory));

        var domainHistory = new BenchmarkHistory(
            benchmarkHistoryId: dbHistory.BenchmarkHistoryId,
            userId: dbHistory.UserId,
            benchmarkName: dbHistory.BenchmarkName,
            savedAt: dbHistory.SavedAt,

            filterIndustryFieldId: dbHistory.FilterIndustryFieldId,
            filterStandardJobRoleId: dbHistory.FilterStandardJobRoleId,
            filterHierarchyLevelId: dbHistory.FilterHierarchyLevelId,
            filterDistrictId: dbHistory.FilterDistrictId,
            filterOblastId: dbHistory.FilterOblastId,
            filterCityId: dbHistory.FilterCityId,
            filterDateStart: dbHistory.FilterDateStart,
            filterDateEnd: dbHistory.FilterDateEnd,
            filterTargetPercentile: dbHistory.FilterTargetPercentile,
            filterGranularity: dbHistory.FilterGranularity,
            filterPeriods: dbHistory.FilterPeriods,

            benchmarkResultJson: dbHistory.BenchmarkResultJson
        );
        
        if (dbHistory.User != null)
        {
            domainHistory.User = UserConverter.ToDomain(dbHistory.User);
        }

        return domainHistory;
    }
    
    public static List<BenchmarkHistory> ToDomainList(IEnumerable<BenchmarkHistoryDbModel> dbHistories)
    {
        if (dbHistories == null)
            return new List<BenchmarkHistory>();

        return dbHistories.Select(ToDomain).ToList();
    }
}
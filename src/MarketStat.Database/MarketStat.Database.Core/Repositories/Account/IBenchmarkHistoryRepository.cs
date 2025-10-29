using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;

namespace MarketStat.Database.Core.Repositories.Account;

public interface IBenchmarkHistoryRepository
{
    Task<BenchmarkHistory> SaveBenchmarkAsync(BenchmarkHistory benchmarkToSave);
    Task<IEnumerable<BenchmarkHistory>> GetBenchmarksByUserIdAsync(int userId);
    Task<BenchmarkHistory> GetBenchmarkHistoryByIdAndUserIdAsync(long benchmarkHistoryId, int userId);
    Task DeleteBenchmarkHistoryAsync(long benchmarkHistoryId, int userId);
}
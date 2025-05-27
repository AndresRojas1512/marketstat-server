using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;

namespace MarketStat.Services.Account.BenchmarkHistoryService;

public interface IBenchmarkHistoryService
{
    Task<long> SaveCurrentUserBenchmarkAsync(SaveBenchmarkRequestDto saveRequestDto, int currentUserId);
    Task<IEnumerable<BenchmarkHistoryDto>> GetCurrentUserBenchmarksAsync(int currentUserId);
    Task<BenchmarkHistoryDto> GetBenchmarkDetailsAsync(long benchmarkHistoryId, int currentUserId);
    Task DeleteCurrentUserBenchmarkAsync(long benchmarkHistoryId, int currentUserId);
}
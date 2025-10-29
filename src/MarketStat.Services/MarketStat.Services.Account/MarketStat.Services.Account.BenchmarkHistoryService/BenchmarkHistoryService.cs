using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Account.BenchmarkHistoryService.Validator;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;

namespace MarketStat.Services.Account.BenchmarkHistoryService;

public class BenchmarkHistoryService : IBenchmarkHistoryService
{
    private readonly IBenchmarkHistoryRepository _benchmarkHistoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BenchmarkHistoryService> _logger;

    public BenchmarkHistoryService(
        IBenchmarkHistoryRepository benchmarkHistoryRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<BenchmarkHistoryService> logger)
    {
        _benchmarkHistoryRepository = benchmarkHistoryRepository ??
                                      throw new ArgumentNullException(nameof(benchmarkHistoryRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<long> SaveCurrentUserBenchmarkAsync(SaveBenchmarkRequestDto saveRequestDto, int currentUserId)
    {
        _logger.LogInformation("User {UserId} attempting to save benchmark: {BenchmarkName}", currentUserId,
            saveRequestDto.BenchmarkName);
        BenchmarkHistoryValidator.ValidateForSave(saveRequestDto);
        var benchmarkToSave = _mapper.Map<BenchmarkHistory>(saveRequestDto);
        benchmarkToSave.UserId = currentUserId;
        var savedBenchmark = await _benchmarkHistoryRepository.SaveBenchmarkAsync(benchmarkToSave);
        
        _logger.LogInformation("Benchmark saved successfully with ID {BenchmarkHistoryId} for User {UserId}",
            savedBenchmark.BenchmarkHistoryId, currentUserId);
        return savedBenchmark.BenchmarkHistoryId;
    }

    public async Task<IEnumerable<BenchmarkHistoryDto>> GetCurrentUserBenchmarksAsync(int currentUserId)
    {
        _logger.LogInformation("Fetching benchmark history for User {UserId}", currentUserId);
        var domainHistories = await _benchmarkHistoryRepository.GetBenchmarksByUserIdAsync(currentUserId);
        var dtoList = _mapper.Map<List<BenchmarkHistoryDto>>(domainHistories);

        if (dtoList.Any())
        {
            _logger.LogInformation("User {UserId} has {Count} benchmarks. Fetching username.", currentUserId,
                dtoList.Count);
            try
            {
                var user = await _userRepository.GetUserByIdAsync(currentUserId);
                if (user != null)
                {
                    dtoList.ForEach(dto => dto.Username = user.Username);
                }
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("User {UserId} not found when populating username for benchmark history list.",
                    currentUserId);
            }
        }

        _logger.LogInformation("Retrieved {Count} benchmark history records for User {UserId}", dtoList.Count,
            currentUserId);
        return dtoList;
    }

    public async Task<BenchmarkHistoryDto> GetBenchmarkDetailsAsync(long benchmarkHistoryId, int currentUserId)
    {
        _logger.LogInformation("Fetching details for benchmark history ID {BenchmarkHistoryId} for User {UserId}",
            benchmarkHistoryId, currentUserId);
        var domainHistory =
            await _benchmarkHistoryRepository.GetBenchmarkHistoryByIdAndUserIdAsync(benchmarkHistoryId, currentUserId);

        var dto = _mapper.Map<BenchmarkHistoryDto>(domainHistory);

        if (string.IsNullOrEmpty(dto.Username))
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(currentUserId);
                if (user != null)
                {
                    dto.Username = user.Username;
                }
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("Could not find User {UserId}", currentUserId);
            }
        }
        _logger.LogInformation("Successfully retrieved details for benchmark {BenchmarkHistoryId}", benchmarkHistoryId);
        return dto;
    }

    public async Task DeleteCurrentUserBenchmarkAsync(long benchmarkHistoryId, int currentUserId)
    {
        _logger.LogInformation("User {UserId} attempting to delete benchmark history ID {BenchmarkHistoryId}", currentUserId, benchmarkHistoryId);
        await _benchmarkHistoryRepository.DeleteBenchmarkHistoryAsync(benchmarkHistoryId, currentUserId);
        _logger.LogInformation("Successfully deleted benchmark history ID {BenchmarkHistoryId} for User {UserId}", benchmarkHistoryId, currentUserId);
    }
}

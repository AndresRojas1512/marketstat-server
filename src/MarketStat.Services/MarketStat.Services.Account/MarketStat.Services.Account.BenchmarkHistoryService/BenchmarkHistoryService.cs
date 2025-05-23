using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Services.Account.BenchmarkHistoryService.Validator;
using Microsoft.Extensions.Logging;

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
        _benchmarkHistoryRepository = benchmarkHistoryRepository ?? throw new ArgumentNullException(nameof(benchmarkHistoryRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<long> SaveCurrentUserBenchmarkAsync(SaveBenchmarkRequestDto saveRequestDto, int currentUserId)
    {
        BenchmarkHistoryValidator.ValidateForSave(saveRequestDto);

        _logger.LogInformation("User {UserId} attempting to save benchmark: {BenchmarkName}", currentUserId, saveRequestDto.BenchmarkName);

        long newHistoryId = await _benchmarkHistoryRepository.SaveBenchmarkAsync(currentUserId, saveRequestDto);

        if (newHistoryId <= 0)
        {
            _logger.LogError("Failed to save benchmark for user {UserId}. Repository returned invalid ID: {NewHistoryId}", currentUserId, newHistoryId);
            throw new ApplicationException("Failed to save benchmark history.");
        }

        _logger.LogInformation("Benchmark saved successfully with ID {BenchmarkHistoryId} for User {UserId}", newHistoryId, currentUserId);
        return newHistoryId;
    }

    public async Task<IEnumerable<BenchmarkHistoryDto>> GetCurrentUserBenchmarksAsync(int currentUserId)
    {
        _logger.LogInformation("Fetching benchmark history for User {UserId}", currentUserId);
        var domainHistories = await _benchmarkHistoryRepository.GetBenchmarksByUserIdAsync(currentUserId);
        
        var dtos = _mapper.Map<IEnumerable<BenchmarkHistoryDto>>(domainHistories).ToList();

        if (dtos.Any())
        {
            var user = await _userRepository.GetUserByIdAsync(currentUserId);
            if (user != null)
            {
                foreach (var dto in dtos)
                {
                    dto.Username = user.Username;
                }
            }
        }
        
        _logger.LogInformation("Retrieved {Count} benchmark history records for User {UserId}", dtos.Count, currentUserId);
        return dtos;
    }

    public async Task<BenchmarkHistoryDto?> GetBenchmarkDetailsAsync(long benchmarkHistoryId, int currentUserId)
    {
        _logger.LogInformation("Fetching details for benchmark history ID {BenchmarkHistoryId} for User {UserId}", benchmarkHistoryId, currentUserId);
        var domainHistory = await _benchmarkHistoryRepository.GetBenchmarkHistoryByIdAndUserIdAsync(benchmarkHistoryId, currentUserId);

        if (domainHistory == null)
        {
            _logger.LogWarning("Benchmark history ID {BenchmarkHistoryId} not found or not owned by User {UserId}", benchmarkHistoryId, currentUserId);
            return null;
        }

        var dto = _mapper.Map<BenchmarkHistoryDto>(domainHistory);
        
        if (dto.UserId == currentUserId && string.IsNullOrEmpty(dto.Username))
        {
             var user = await _userRepository.GetUserByIdAsync(currentUserId);
             if (user != null) dto.Username = user.Username;
        }

        return dto;
    }
    
    public async Task<bool> DeleteCurrentUserBenchmarkAsync(long benchmarkHistoryId, int currentUserId)
    {
        _logger.LogInformation("User {UserId} attempting to delete benchmark history ID {BenchmarkHistoryId}", currentUserId, benchmarkHistoryId);
        bool deleted = await _benchmarkHistoryRepository.DeleteBenchmarkHistoryAsync(benchmarkHistoryId, currentUserId);
        if (!deleted)
        {
            _logger.LogWarning("Failed to delete benchmark history ID {BenchmarkHistoryId} for User {UserId}. Record not found or not owned by user.", benchmarkHistoryId, currentUserId);
        }
        else
        {
            _logger.LogInformation("Successfully deleted benchmark history ID {BenchmarkHistoryId} for User {UserId}", benchmarkHistoryId, currentUserId);
        }
        return deleted;
    }
}
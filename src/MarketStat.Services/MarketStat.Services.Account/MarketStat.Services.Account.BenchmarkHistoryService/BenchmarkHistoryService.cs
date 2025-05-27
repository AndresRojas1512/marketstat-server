using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Account.BenchmarkHistoryService.Validator;
using MarketStat.Services.Dimensions.DimCityService;
using MarketStat.Services.Dimensions.DimFederalDistrictService;
using MarketStat.Services.Dimensions.DimHierarchyLevelService;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using MarketStat.Services.Dimensions.DimOblastService;
using MarketStat.Services.Dimensions.DimStandardJobRoleService;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Account.BenchmarkHistoryService;

public class BenchmarkHistoryService : IBenchmarkHistoryService
{
    private readonly IBenchmarkHistoryRepository _benchmarkHistoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BenchmarkHistoryService> _logger;

    private readonly IDimIndustryFieldService _dimIndustryFieldService;
    private readonly IDimStandardJobRoleService _dimStandardJobRoleService;
    private readonly IDimHierarchyLevelService _dimHierarchyLevelService;
    private readonly IDimFederalDistrictService _dimFederalDistrictService;
    private readonly IDimOblastService _dimOblastService;
    private readonly IDimCityService _dimCityService;

    public BenchmarkHistoryService(
        IBenchmarkHistoryRepository benchmarkHistoryRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<BenchmarkHistoryService> logger,
        IDimIndustryFieldService dimIndustryFieldService,
        IDimStandardJobRoleService dimStandardJobRoleService,
        IDimHierarchyLevelService dimHierarchyLevelService,
        IDimFederalDistrictService dimFederalDistrictService,
        IDimOblastService dimOblastService,
        IDimCityService dimCityService)
    {
        _benchmarkHistoryRepository = benchmarkHistoryRepository ??
                                      throw new ArgumentNullException(nameof(benchmarkHistoryRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dimIndustryFieldService =
            dimIndustryFieldService ?? throw new ArgumentNullException(nameof(dimIndustryFieldService));
        _dimStandardJobRoleService = dimStandardJobRoleService ??
                                     throw new ArgumentNullException(nameof(dimStandardJobRoleService));
        _dimHierarchyLevelService = dimHierarchyLevelService ??
                                    throw new ArgumentNullException(nameof(dimHierarchyLevelService));
        _dimFederalDistrictService = dimFederalDistrictService ??
                                     throw new ArgumentNullException(nameof(dimFederalDistrictService));
        _dimOblastService = dimOblastService ?? throw new ArgumentNullException(nameof(dimOblastService));
        _dimCityService = dimCityService ?? throw new ArgumentNullException(nameof(dimCityService));
    }

    public async Task<long> SaveCurrentUserBenchmarkAsync(SaveBenchmarkRequestDto saveRequestDto, int currentUserId)
    {
        BenchmarkHistoryValidator.ValidateForSave(saveRequestDto);
        _logger.LogInformation("User {UserId} attempting to save benchmark: {BenchmarkName}", currentUserId,
            saveRequestDto.BenchmarkName);
        long newHistoryId = await _benchmarkHistoryRepository.SaveBenchmarkAsync(currentUserId, saveRequestDto);
        _logger.LogInformation("Benchmark saved successfully with ID {BenchmarkHistoryId} for User {UserId}",
            newHistoryId, currentUserId);
        return newHistoryId;
    }

    public async Task<IEnumerable<BenchmarkHistoryDto>> GetCurrentUserBenchmarksAsync(int currentUserId)
    {
        _logger.LogInformation("Fetching benchmark history for User {UserId}", currentUserId);
        var domainHistories = await _benchmarkHistoryRepository.GetBenchmarksByUserIdAsync(currentUserId);

        var dtoList = new List<BenchmarkHistoryDto>();
        User? user = null;

        if (domainHistories.Any())
        {
            try
            {
                user = await _userRepository.GetUserByIdAsync(currentUserId);
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("User {UserId} not found when trying to populate username for benchmark history.",
                    currentUserId);
            }
        }

        foreach (var domainHistory in domainHistories)
        {
            var dto = _mapper.Map<BenchmarkHistoryDto>(domainHistory);
            await PopulateFilterNamesInDtoAsync(dto, domainHistory);

            if (user != null)
            {
                dto.Username = user.Username;
            }
            else if (domainHistory.User != null)
            {
                dto.Username = domainHistory.User.Username;
            }
            dtoList.Add(dto);
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
        await PopulateFilterNamesInDtoAsync(dto, domainHistory);

        if (domainHistory.User != null)
        {
            dto.Username = domainHistory.User.Username;
        }
        else if (string.IsNullOrEmpty(dto.Username))
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
                _logger.LogWarning(
                    "User {UserId} not found when trying to populate username for benchmark history details {BenchmarkHistoryId}.",
                    currentUserId, benchmarkHistoryId);
            }
        }
        return dto;
    }

    public async Task DeleteCurrentUserBenchmarkAsync(long benchmarkHistoryId, int currentUserId)
    {
        _logger.LogInformation("User {UserId} attempting to delete benchmark history ID {BenchmarkHistoryId}", currentUserId, benchmarkHistoryId);
        await _benchmarkHistoryRepository.DeleteBenchmarkHistoryAsync(benchmarkHistoryId, currentUserId);
        _logger.LogInformation("Successfully deleted benchmark history ID {BenchmarkHistoryId} for User {UserId}", benchmarkHistoryId, currentUserId);
    }

    private async Task PopulateFilterNamesInDtoAsync(BenchmarkHistoryDto dto, BenchmarkHistory domainHistory)
    {
        if (domainHistory.FilterIndustryFieldId.HasValue)
        {
            try
            {
                var item = await _dimIndustryFieldService.GetIndustryFieldByIdAsync(domainHistory.FilterIndustryFieldId
                    .Value);
                dto.FilterIndustryFieldName = item?.IndustryFieldName;
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("IndustryField ID {Id} from benchmark history not found in dimension table.",
                    domainHistory.FilterIndustryFieldId.Value);
                dto.FilterIndustryFieldName = $"<ID: {domainHistory.FilterIndustryFieldId} Not Found>";
            }
        }

        if (domainHistory.FilterStandardJobRoleId.HasValue)
        {
            try
            {
                var item = await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(domainHistory
                    .FilterStandardJobRoleId.Value);
                dto.FilterStandardJobRoleTitle = item?.StandardJobRoleTitle;
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("StandardJobRole ID {Id} from benchmark history not found.",
                    domainHistory.FilterStandardJobRoleId.Value);
                dto.FilterStandardJobRoleTitle = $"<ID: {domainHistory.FilterStandardJobRoleId} Not Found>";
            }
        }

        if (domainHistory.FilterHierarchyLevelId.HasValue)
        {
            try
            {
                var item = await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(domainHistory
                    .FilterHierarchyLevelId.Value);
                dto.FilterHierarchyLevelName = item?.HierarchyLevelName;
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("HierarchyLevel ID {Id} from benchmark history not found.",
                    domainHistory.FilterHierarchyLevelId.Value);
                dto.FilterHierarchyLevelName = $"<ID: {domainHistory.FilterHierarchyLevelId} Not Found>";
            }
        }

        if (domainHistory.FilterDistrictId.HasValue)
        {
            try
            {
                var item = await _dimFederalDistrictService.GetDistrictByIdAsync(domainHistory.FilterDistrictId.Value);
                dto.FilterDistrictName = item?.DistrictName;
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("FederalDistrict ID {Id} from benchmark history not found.",
                    domainHistory.FilterDistrictId.Value);
                dto.FilterDistrictName = $"<ID: {domainHistory.FilterDistrictId} Not Found>";
            }
        }

        if (domainHistory.FilterOblastId.HasValue)
        {
            try
            {
                var item = await _dimOblastService.GetOblastByIdAsync(domainHistory.FilterOblastId.Value);
                dto.FilterOblastName = item?.OblastName;
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("Oblast ID {Id} from benchmark history not found.",
                    domainHistory.FilterOblastId.Value);
                dto.FilterOblastName = $"<ID: {domainHistory.FilterOblastId} Not Found>";
            }
        }

        if (domainHistory.FilterCityId.HasValue)
        {
            try
            {
                var item = await _dimCityService.GetCityByIdAsync(domainHistory.FilterCityId.Value);
                dto.FilterCityName = item?.CityName;
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("City ID {Id} from benchmark history not found.", domainHistory.FilterCityId.Value);
                dto.FilterCityName = $"<ID: {domainHistory.FilterCityId} Not Found>";
            }
        }
    }
}

using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Common.Validators.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimDateService;

public class DimDateService : IDimDateService
{
    private readonly IDimDateRepository _dimDateRepository;
    private readonly ILogger<DimDateService> _logger;

    public DimDateService(IDimDateRepository dimDateRepository, ILogger<DimDateService> logger)
    {
        _dimDateRepository = dimDateRepository;
        _logger = logger;
    }
    
    public async Task<DimDate> CreateDateAsync(DateOnly fullDate)
    {
        DimDateValidator.ValidateForCreate(fullDate);
        
        var year    = fullDate.Year;
        var month   = fullDate.Month;
        var quarter = (month - 1) / 3 + 1;
        
        var date = new DimDate(0, fullDate, year, quarter, month);

        try
        {
            await _dimDateRepository.AddDateAsync(date);
            _logger.LogInformation("Created date {DateId}", date.DateId);
            return date;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict creating date {FullDate}", date.FullDate);
            throw;
        }
    }
    
    public async Task<DimDate> GetDateByIdAsync(int dateId)
    {
        try
        {
            return await _dimDateRepository.GetDateByIdAsync(dateId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Date {DateId} not found", dateId);
            throw;
        }
    }
    
    public async Task<IEnumerable<DimDate>> GetAllDatesAsync()
    {
        var list = await _dimDateRepository.GetAllDatesAsync();
        _logger.LogInformation("Fetched {Count} date records", list.Count());
        return list;
    }
    
    public async Task<DimDate> UpdateDateAsync(int dateId, DateOnly fullDate)
    {
        DimDateValidator.ValidateForUpdate(dateId, fullDate);
        try
        {
            var existing = await _dimDateRepository.GetDateByIdAsync(dateId);

            existing.FullDate = fullDate;
            existing.Year = fullDate.Year;
            existing.Month = fullDate.Month;
            existing.Quarter = (fullDate.Month - 1) / 3 + 1;

            await _dimDateRepository.UpdateDateAsync(existing);
            _logger.LogInformation("Updated date {DateId}", dateId);
            return existing;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update date {DateId} not found", dateId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Cannot update date {DateId}: duplicate", dateId);
            throw;
        }
    }
    
    public async Task DeleteDateAsync(int dateId)
    {
        try
        {
            await _dimDateRepository.DeleteDateAsync(dateId);
            _logger.LogInformation("Deleted date {DateId}", dateId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete date {DateId}: not found", dateId);
            throw;
        }
    }
}
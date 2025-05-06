using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimDateService.Validators;
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
            _logger.LogInformation("Created DimDate {DateId}", date.DateId);
            return date;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DimDate for {Date}", fullDate);
            throw new Exception($"A dim_date row for {fullDate} already exists.");
        }
    }
    
    public async Task<DimDate> GetDateByIdAsync(int dateId)
    {
        try
        {
            return await _dimDateRepository.GetDateByIdAsync(dateId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "DimDate {DateId} not found", dateId);
            throw new Exception($"Date with ID {dateId} was not found.");
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
            _logger.LogInformation("Updated DimDate {DateId}", dateId);
            return existing;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update – DimDate {DateId} not found", dateId);
            throw new Exception($"Cannot update: date {dateId} not found.");
        }
    }
    
    public async Task DeleteDateAsync(int dateId)
    {
        try
        {
            await _dimDateRepository.DeleteDateAsync(dateId);
            _logger.LogInformation("Deleted DimDate {DateId}", dateId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete - DimDate {DateId} not found", dateId);
            throw new Exception($"Cannot delete: date {dateId} not found.");
        }
    }
}
using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimDate;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimDate;

public class DimDateDataConsumer :
    IConsumer<IPersistDimDateCommand>,
    IConsumer<IPersistDimDateUpdateCommand>,
    IConsumer<IPersistDimDateDeleteCommand>
{
    private readonly IDimDateRepository _repository;
    private readonly ILogger<DimDateDataConsumer> _logger;

    public DimDateDataConsumer(IDimDateRepository repository, ILogger<DimDateDataConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IPersistDimDateCommand> context)
    {
        var msg = context.Message;
        
        var year = msg.FullDate.Year;
        var month = msg.FullDate.Month;
        var quarter = (month - 1) / 3 + 1;
        
        var date = new Common.Core.MarketStat.Common.Core.Dimensions.DimDate(0, msg.FullDate, year, quarter, month);

        try
        {
            await _repository.AddDateAsync(date);
            _logger.LogInformation("Data: Saved Date {DateId}", date.DateId);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Data: Duplicate Date {FullDate}", date.FullDate);
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimDateUpdateCommand> context)
    {
        var msg = context.Message;

        var year = msg.FullDate.Year;
        var month = msg.FullDate.Month;
        var quarter = (month - 1) / 3 + 1;

        var date = new Common.Core.MarketStat.Common.Core.Dimensions.DimDate(msg.DateId, msg.FullDate, year, quarter, month);
        try
        {
            await _repository.UpdateDateAsync(date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update Failed");
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimDateDeleteCommand> context)
    {
        try
        {
            await _repository.DeleteDateAsync(context.Message.DateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete Failed");
        }
    }
}
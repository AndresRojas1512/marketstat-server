using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimIndustryField;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimIndustryField;

public class DimIndustryFieldDataConsumer :
    IConsumer<IPersistDimIndustryFieldCommand>,
    IConsumer<IPersistDimIndustryFieldUpdateCommand>,
    IConsumer<IPersistDimIndustryFieldDeleteCommand>
{
    private readonly IDimIndustryFieldRepository _repository;
    private readonly ILogger<DimIndustryFieldDataConsumer> _logger;
    
    public DimIndustryFieldDataConsumer(IDimIndustryFieldRepository repository, ILogger<DimIndustryFieldDataConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<IPersistDimIndustryFieldCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Saving Industry Field {Code}", msg.IndustryFieldCode);
        
        var industry = new Common.Core.MarketStat.Common.Core.Dimensions.DimIndustryField(0, msg.IndustryFieldCode, msg.IndustryFieldName);
        try
        {
            await _repository.AddIndustryFieldAsync(industry);
            _logger.LogInformation("Data: Saved Industry Field ID {Id}", industry.IndustryFieldId);
        }
        catch (ConflictException)
        {
            _logger.LogWarning("Data: Duplicate Industry Field.");
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimIndustryFieldUpdateCommand> context)
    {
        var msg = context.Message;
        var industry = new Common.Core.MarketStat.Common.Core.Dimensions.DimIndustryField(msg.IndustryFieldId, msg.IndustryFieldCode, msg.IndustryFieldName);
        try
        {
            await _repository.UpdateIndustryFieldAsync(industry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update Failed");
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimIndustryFieldDeleteCommand> context)
    {
        try
        {
            await _repository.DeleteIndustryFieldAsync(context.Message.IndustryFieldId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete Failed");
        }
    }
}
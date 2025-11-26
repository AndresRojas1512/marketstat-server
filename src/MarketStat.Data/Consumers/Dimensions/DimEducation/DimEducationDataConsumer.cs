using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimEducation;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimEducation;

public class DimEducationDataConsumer :
    IConsumer<IPersistDimEducationCommand>,
    IConsumer<IPersistDimEducationUpdateCommand>,
    IConsumer<IPersistDimEducationDeleteCommand>
{
    private readonly IDimEducationRepository _repository;
    private readonly ILogger<DimEducationDataConsumer> _logger;
    
    public DimEducationDataConsumer(IDimEducationRepository repository, ILogger<DimEducationDataConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<IPersistDimEducationCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Saving Education {Code}", msg.SpecialtyCode);
        var education = new Common.Core.MarketStat.Common.Core.Dimensions.DimEducation(0, msg.SpecialtyName,
            msg.SpecialtyCode, msg.EducationLevelName);
        try
        {
            await _repository.AddEducationAsync(education);
            _logger.LogInformation("Data: Saved Education ID {Id}", education.EducationId);
        }
        catch (ConflictException)
        {
            _logger.LogWarning("Data: Duplicate Education.");
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimEducationUpdateCommand> context)
    {
        var msg = context.Message;
        var education = new Common.Core.MarketStat.Common.Core.Dimensions.DimEducation(msg.EducationId,
            msg.SpecialtyName, msg.SpecialtyCode, msg.EducationLevelName);
        try
        {
            await _repository.UpdateEducationAsync(education);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update Failed");
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimEducationDeleteCommand> context)
    {
        try
        {
            await _repository.DeleteEducationAsync(context.Message.EducationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete Failed");
        }
    }
}
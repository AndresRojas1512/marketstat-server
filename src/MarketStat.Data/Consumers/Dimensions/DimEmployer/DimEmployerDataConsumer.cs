using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimEmployer;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimEmployer;

public class DimEmployerDataConsumer :
    IConsumer<IPersistDimEmployerCommand>,
    IConsumer<IPersistDimEmployerUpdateCommand>,
    IConsumer<IPersistDimEmployerDeleteCommand>
{
    private readonly IDimEmployerRepository _repository;
    private readonly ILogger<DimEmployerDataConsumer> _logger;

    public DimEmployerDataConsumer(IDimEmployerRepository repository, ILogger<DimEmployerDataConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<IPersistDimEmployerCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Saving Employer {Name}...", msg.EmployerName);
        var employer = new Common.Core.MarketStat.Common.Core.Dimensions.DimEmployer(0, msg.EmployerName, msg.Inn,
            msg.Ogrn, msg.Kpp, msg.RegistrationDate, msg.LegalAddress, msg.ContactEmail, msg.ContactPhone,
            msg.IndustryFieldId);
        try
        {
            await _repository.AddEmployerAsync(employer);
            _logger.LogInformation("Data: Saved Employer ID {Id}", employer.EmployerId);
        }
        catch (ConflictException)
        {
            _logger.LogWarning("Data: Duplicate Employer.");
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimEmployerUpdateCommand> context)
    {
        var msg = context.Message;
        var employer = new Common.Core.MarketStat.Common.Core.Dimensions.DimEmployer(msg.EmployerId, msg.EmployerName,
            msg.Inn, msg.Ogrn, msg.Kpp, msg.RegistrationDate, msg.LegalAddress, msg.ContactEmail, msg.ContactPhone,
            msg.IndustryFieldId);
        try
        {
            await _repository.UpdateEmployerAsync(employer);
            _logger.LogInformation("Data: Updated Employer ID {Id}", msg.EmployerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update Failed");
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimEmployerDeleteCommand> context)
    {
        try
        {
            await _repository.DeleteEmployerAsync(context.Message.EmployerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete Failed");
        }
    }
}
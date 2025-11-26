using MarketStat.Common.Validators.Dimensions;
using MarketStat.Contracts.Dimensions.DimEmployer;
using MassTransit;

namespace MarketStat.Domain.Consumers.Dimensions;

public class DimEmployerDomainConsumer :
    IConsumer<ISubmitDimEmployerCommand>,
    IConsumer<ISubmitDimEmployerUpdateCommand>,
    IConsumer<ISubmitDimEmployerDeleteCommand>
{
    private readonly ILogger<DimEmployerDomainConsumer> _logger;
    
    public DimEmployerDomainConsumer(ILogger<DimEmployerDomainConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ISubmitDimEmployerCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating Employer {Name}", msg.EmployerName);
        try
        {
            DimEmployerValidator.ValidateForCreate(msg.EmployerName, msg.Inn, msg.Ogrn, msg.Kpp, msg.RegistrationDate,
                msg.LegalAddress, msg.ContactEmail, msg.ContactPhone, msg.IndustryFieldId);
            await context.Publish<IPersistDimEmployerCommand>(new
            {
                msg.EmployerName, msg.Inn, msg.Ogrn, msg.Kpp, msg.RegistrationDate, msg.LegalAddress, msg.ContactEmail,
                msg.ContactPhone, msg.IndustryFieldId
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimEmployerUpdateCommand> context)
    {
        var msg = context.Message;
        try
        {
            DimEmployerValidator.ValidateForUpdate(msg.EmployerId, msg.EmployerName, msg.Inn, msg.Ogrn, msg.Kpp,
                msg.RegistrationDate, msg.LegalAddress, msg.ContactEmail, msg.ContactPhone, msg.IndustryFieldId);

            await context.Publish<IPersistDimEmployerUpdateCommand>(new
            {
                msg.EmployerId, msg.EmployerName, msg.Inn, msg.Ogrn, msg.Kpp, msg.RegistrationDate, msg.LegalAddress,
                msg.ContactEmail, msg.ContactPhone, msg.IndustryFieldId
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Update Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimEmployerDeleteCommand> context)
    {
        if (context.Message.EmployerId <= 0)
        {
            return;
        }
        await context.Publish<IPersistDimEmployerDeleteCommand>(new
        {
            context.Message.EmployerId
        });
    }
}
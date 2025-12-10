using MarketStat.Common.Validators.Dimensions;
using MarketStat.Contracts.Dimensions.DimLocation;
using MassTransit;

namespace MarketStat.Domain.Consumers.Dimensions;

public class DimLocationDomainConsumer :
    IConsumer<ISubmitDimLocationCommand>,
    IConsumer<ISubmitDimLocationUpdateCommand>,
    IConsumer<ISubmitDimLocationDeleteCommand>
{
    private readonly ILogger<DimLocationDomainConsumer> _logger;

    public DimLocationDomainConsumer(ILogger<DimLocationDomainConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ISubmitDimLocationCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating Location {City}", msg.CityName);
        try
        {
            DimLocationValidator.ValidateForCreate(msg.CityName, msg.OblastName, msg.DistrictName);
            
            await context.Publish<IPersistDimLocationCommand>(new
            {
                msg.CityName, msg.OblastName, msg.DistrictName
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimLocationUpdateCommand> context)
    {
        var msg = context.Message;
        try
        {
            DimLocationValidator.ValidateForUpdate(msg.LocationId, msg.CityName, msg.OblastName, msg.DistrictName);
            
            await context.Publish<IPersistDimLocationUpdateCommand>(new
            {
                msg.LocationId, msg.CityName, msg.OblastName, msg.DistrictName
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Update Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimLocationDeleteCommand> context)
    {
        if (context.Message.LocationId <= 0) return;
        await context.Publish<IPersistDimLocationDeleteCommand>(new { context.Message.LocationId });
    }
}
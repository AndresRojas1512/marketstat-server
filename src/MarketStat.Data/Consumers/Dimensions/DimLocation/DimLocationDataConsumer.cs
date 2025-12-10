using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimLocation;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimLocation;

public class DimLocationDataConsumer :
    IConsumer<IPersistDimLocationCommand>,
    IConsumer<IPersistDimLocationUpdateCommand>,
    IConsumer<IPersistDimLocationDeleteCommand>
{
    private readonly IDimLocationRepository _repository;
    private readonly ILogger<DimLocationDataConsumer> _logger;

    public DimLocationDataConsumer(IDimLocationRepository repository, ILogger<DimLocationDataConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IPersistDimLocationCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Saving Location {City}...", msg.CityName);
        
        var location = new Common.Core.MarketStat.Common.Core.Dimensions.DimLocation(0, msg.CityName, msg.OblastName, msg.DistrictName);
        try
        {
            await _repository.AddLocationAsync(location);
            _logger.LogInformation("Data: Saved Location ID {Id}", location.LocationId);
        }
        catch (ConflictException)
        {
            _logger.LogWarning("Data: Duplicate Location.");
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimLocationUpdateCommand> context)
    {
        var msg = context.Message;
        var location = new Common.Core.MarketStat.Common.Core.Dimensions.DimLocation(msg.LocationId, msg.CityName, msg.OblastName, msg.DistrictName);
        try
        {
            await _repository.UpdateLocationAsync(location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update Failed");
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimLocationDeleteCommand> context)
    {
        try
        {
            await _repository.DeleteLocationAsync(context.Message.LocationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete Failed");
        }
    }
}
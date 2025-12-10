using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimLocation;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimLocation;

public class DimLocationReadConsumer :
    IConsumer<IGetDimLocationRequest>,
    IConsumer<IGetAllDimLocationsRequest>,
    IConsumer<IGetDistrictsRequest>,
    IConsumer<IGetOblastsRequest>,
    IConsumer<IGetCitiesRequest>
{
    private readonly IDimLocationRepository _repository;

    public DimLocationReadConsumer(IDimLocationRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<IGetDimLocationRequest> context)
    {
        try
        {
            var loc = await _repository.GetLocationByIdAsync(context.Message.LocationId);
            await context.RespondAsync<IGetDimLocationResponse>(new
            {
                loc.LocationId,
                loc.CityName,
                loc.OblastName,
                loc.DistrictName
            });
        }
        catch (NotFoundException)
        {
            await context.RespondAsync<IDimLocationNotFoundResponse>(new
            {
                context.Message.LocationId
            });
        }
    }

    public async Task Consume(ConsumeContext<IGetAllDimLocationsRequest> context)
    {
        var list = await _repository.GetAllLocationsAsync();
        var responseList = list.Select(l => new
        {
            l.LocationId,
            l.CityName,
            l.OblastName,
            l.DistrictName
        }).ToList();
        await context.RespondAsync<IGetAllDimLocationsResponse>(new
        {
            Locations = responseList
        });
    }


    public async Task Consume(ConsumeContext<IGetDistrictsRequest> context)
    {
        var districts = await _repository.GetDistinctDistrictsAsync();
        await context.RespondAsync<IGetDistrictsResponse>(new
        {
            Districts = districts.ToList()
        });
    }

    public async Task Consume(ConsumeContext<IGetOblastsRequest> context)
    {
        var oblasts = await _repository.GetDistinctOblastsAsync(context.Message.DistrictName);
        await context.RespondAsync<IGetOblastsResponse>(new
        {
            Oblasts = oblasts.ToList()
        });
    }

    public async Task Consume(ConsumeContext<IGetCitiesRequest> context)
    {
        var cities = await _repository.GetDistinctCitiesAsync(context.Message.OblastName);
        await context.RespondAsync<IGetCitiesResponse>(new
        {
            Cities = cities.ToList()
        });
    }
}
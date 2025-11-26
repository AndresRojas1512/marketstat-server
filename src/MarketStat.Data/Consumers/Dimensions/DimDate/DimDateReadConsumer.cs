using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimDate;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimDate;

public class DimDateReadConsumer :
    IConsumer<IGetDimDateRequest>,
    IConsumer<IGetAllDimDatesRequest>
{
    private readonly IDimDateRepository _repository;

    public DimDateReadConsumer(IDimDateRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<IGetDimDateRequest> context)
    {
        try
        {
            var date = await _repository.GetDateByIdAsync(context.Message.DateId);
            await context.RespondAsync<IGetDimDateResponse>(new
            {
                date.DateId, date.FullDate, date.Year, date.Month, date.Quarter
            });
        }
        catch (NotFoundException)
        {
            await context.RespondAsync<IDimDateNotFoundResponse>(new
            {
                context.Message.DateId
            });
        }
    }

    public async Task Consume(ConsumeContext<IGetAllDimDatesRequest> context)
    {
        var dates = await _repository.GetAllDatesAsync();
        var responseList = dates.Select(d => new
        {
            d.DateId, d.FullDate, d.Year, d.Month, d.Quarter
        }).ToList();
        await context.RespondAsync<IGetAllDimDatesResponse>(new
        {
            Dates = responseList
        });
    }
}
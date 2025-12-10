using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimIndustryField;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimIndustryField;

public class DimIndustryFieldReadConsumer :
    IConsumer<IGetDimIndustryFieldRequest>,
    IConsumer<IGetAllDimIndustryFieldsRequest>
{
    private readonly IDimIndustryFieldRepository _repository;
    
    public DimIndustryFieldReadConsumer(IDimIndustryFieldRepository repository)
    {
        _repository = repository;
    }
    
    public async Task Consume(ConsumeContext<IGetDimIndustryFieldRequest> context)
    {
        try
        {
            var ind = await _repository.GetIndustryFieldByIdAsync(context.Message.IndustryFieldId);
            await context.RespondAsync<IGetDimIndustryFieldResponse>(new 
            {
                ind.IndustryFieldId,
                ind.IndustryFieldCode,
                ind.IndustryFieldName
            });
        }
        catch (NotFoundException)
        {
            await context.RespondAsync<IDimIndustryFieldNotFoundResponse>(new
            {
                context.Message.IndustryFieldId
            });
        }
    }
    
    public async Task Consume(ConsumeContext<IGetAllDimIndustryFieldsRequest> context)
    {
        var list = await _repository.GetAllIndustryFieldsAsync();
        var responseList = list.Select(i => new 
        { 
            i.IndustryFieldId, i.IndustryFieldCode, i.IndustryFieldName 
        }).ToList();
        
        await context.RespondAsync<IGetAllDimIndustryFieldsResponse>(new
        {
            IndustryFields = responseList
        });
    }
}
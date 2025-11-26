using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimEducation;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimEducation;

public class DimEducationReadConsumer :
    IConsumer<IGetDimEducationRequest>,
    IConsumer<IGetAllDimEducationsRequest>
{
    private readonly IDimEducationRepository _repository;
    
    public DimEducationReadConsumer(IDimEducationRepository repository)
    {
        _repository = repository;
    }
    
    public async Task Consume(ConsumeContext<IGetDimEducationRequest> context)
    {
        try
        {
            var edu = await _repository.GetEducationByIdAsync(context.Message.EducationId);
            await context.RespondAsync<IGetDimEducationResponse>(new 
            {
                edu.EducationId, edu.SpecialtyName, edu.SpecialtyCode, edu.EducationLevelName
            });
        }
        catch (NotFoundException)
        {
            await context.RespondAsync<IDimEducationNotFoundResponse>(new
            {
                context.Message.EducationId
            });
        }
    }
    
    public async Task Consume(ConsumeContext<IGetAllDimEducationsRequest> context)
    {
        var list = await _repository.GetAllEducationsAsync();
        var responseList = list.Select(e => new 
        { 
            e.EducationId, e.SpecialtyName, e.SpecialtyCode, e.EducationLevelName 
        }).ToList();
        await context.RespondAsync<IGetAllDimEducationsResponse>(new
        {
            Educations = responseList
        });
    }
}
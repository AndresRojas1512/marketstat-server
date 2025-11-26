using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimEmployer;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimEmployer;

public class DimEmployerReadConsumer :
    IConsumer<IGetDimEmployerRequest>,
    IConsumer<IGetAllDimEmployersRequest>
{
    private readonly IDimEmployerRepository _repository;
    
    public DimEmployerReadConsumer(IDimEmployerRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<IGetDimEmployerRequest> context)
    {
        try
        {
            var emp = await _repository.GetEmployerByIdAsync(context.Message.EmployerId);
            await context.RespondAsync<IGetDimEmployerResponse>(new
            {
                emp.EmployerId, emp.EmployerName, emp.Inn, emp.Ogrn, emp.Kpp, emp.RegistrationDate, emp.LegalAddress,
                emp.ContactEmail, emp.ContactPhone, emp.IndustryFieldId
            });
        }
        catch (NotFoundException)
        {
            await context.RespondAsync<IDimEmployerNotFoundResponse>(new
            {
                context.Message.EmployerId
            });
        }
    }

    public async Task Consume(ConsumeContext<IGetAllDimEmployersRequest> context)
    {
        var list = await _repository.GetAllEmployersAsync();
        var responseList = list.Select(emp => new 
        { 
            emp.EmployerId, emp.EmployerName, emp.Inn, emp.Ogrn, emp.Kpp, emp.RegistrationDate, emp.LegalAddress, emp.ContactEmail, emp.ContactPhone, emp.IndustryFieldId
        }).ToList();
        await context.RespondAsync<IGetAllDimEmployersResponse>(new
        {
            Employers = responseList
        });
    }
}
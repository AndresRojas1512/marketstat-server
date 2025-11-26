using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimEmployee;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimEmployee;

public class DimEmployeeReadConsumer :
    IConsumer<IGetDimEmployeeRequest>,
    IConsumer<IGetAllDimEmployeesRequest>
{
    private readonly IDimEmployeeRepository _repository;
    
    public DimEmployeeReadConsumer(IDimEmployeeRepository repository)
    {
        _repository = repository;
    }
    
    public async Task Consume(ConsumeContext<IGetDimEmployeeRequest> context)
    {
        try
        {
            var emp = await _repository.GetEmployeeByIdAsync(context.Message.EmployeeId);
            await context.RespondAsync<IGetDimEmployeeResponse>(new 
            {
                emp.EmployeeId, emp.EmployeeRefId, emp.BirthDate, emp.CareerStartDate, emp.Gender, emp.EducationId, emp.GraduationYear
            });
        }
        catch (NotFoundException)
        {
            await context.RespondAsync<IDimEmployeeNotFoundResponse>(new
            {
                context.Message.EmployeeId
            });
        }
    }

    public async Task Consume(ConsumeContext<IGetAllDimEmployeesRequest> context)
    {
        var list = await _repository.GetAllEmployeesAsync();
        var responseList = list.Select(emp => new 
        { 
            emp.EmployeeId, emp.EmployeeRefId, emp.BirthDate, emp.CareerStartDate, emp.Gender, emp.EducationId, emp.GraduationYear
        }).ToList();
        await context.RespondAsync<IGetAllDimEmployeesResponse>(new
        {
            Employees = responseList
        });
    }
}
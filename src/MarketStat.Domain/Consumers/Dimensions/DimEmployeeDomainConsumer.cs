using MarketStat.Common.Validators.Dimensions;
using MarketStat.Contracts.Dimensions.DimEmployee;
using MassTransit;

namespace MarketStat.Domain.Consumers.Dimensions;

public class DimEmployeeDomainConsumer :
    IConsumer<ISubmitDimEmployeeCommand>,
    IConsumer<ISubmitDimEmployeeUpdateCommand>,
    IConsumer<ISubmitDimEmployeeDeleteCommand>,
    IConsumer<ISubmitDimEmployeePartialUpdateCommand>
{
    private readonly ILogger<DimEmployeeDomainConsumer> _logger;

    public DimEmployeeDomainConsumer(ILogger<DimEmployeeDomainConsumer> logger)
    {
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<ISubmitDimEmployeeCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating Employee {RefId}", msg.EmployeeRefId);
        try
        {
            DimEmployeeValidator.ValidateForCreate(msg.EmployeeRefId, msg.BirthDate, msg.CareerStartDate, msg.Gender,
                msg.EducationId, msg.GraduationYear);
            await context.Publish<IPersistDimEmployeeCommand>(new
            {
                msg.EmployeeRefId, msg.BirthDate, msg.CareerStartDate, msg.Gender, msg.EducationId, msg.GraduationYear
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimEmployeeUpdateCommand> context)
    {
        var msg = context.Message;
        try
        {
            DimEmployeeValidator.ValidateForUpdate(msg.EmployeeId, msg.EmployeeRefId, msg.BirthDate,
                msg.CareerStartDate, msg.Gender, msg.EducationId, msg.GraduationYear);
            await context.Publish<IPersistDimEmployeeUpdateCommand>(new
            {
                msg.EmployeeId, msg.EmployeeRefId, msg.BirthDate, msg.CareerStartDate, msg.Gender, msg.EducationId, msg.GraduationYear
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Update Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimEmployeePartialUpdateCommand> context)
    {
        if (context.Message.EmployeeId <= 0)
        {
            return;
        }
        await context.Publish<IPersistDimEmployeePartialUpdateCommand>(new 
        { 
             context.Message.EmployeeId, 
             context.Message.EmployeeRefId, 
             context.Message.CareerStartDate,
             context.Message.EducationId,
             context.Message.GraduationYear
        });
    }

    public async Task Consume(ConsumeContext<ISubmitDimEmployeeDeleteCommand> context)
    {
        if (context.Message.EmployeeId <= 0)
        {
            return;
        }
        await context.Publish<IPersistDimEmployeeDeleteCommand>(new
        {
            context.Message.EmployeeId
        });
    }
}
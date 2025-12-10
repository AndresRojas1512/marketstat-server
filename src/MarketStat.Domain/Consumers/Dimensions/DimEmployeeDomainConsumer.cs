using MarketStat.Common.Validators.Dimensions;
using MarketStat.Contracts.Dimensions.DimEducation;
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
    private readonly IRequestClient<IGetDimEducationRequest> _educationClient;

    public DimEmployeeDomainConsumer(ILogger<DimEmployeeDomainConsumer> logger, IRequestClient<IGetDimEducationRequest> educationClient)
    {
        _logger = logger;
        _educationClient = educationClient;
    }
    
    public async Task Consume(ConsumeContext<ISubmitDimEmployeeCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating Employee {RefId}", msg.EmployeeRefId);
        try
        {
            DimEmployeeValidator.ValidateForCreate(msg.EmployeeRefId, msg.BirthDate, msg.CareerStartDate, msg.Gender,
                msg.EducationId, msg.GraduationYear);
            if (msg.EducationId.HasValue)
            {
                var response =
                    await _educationClient.GetResponse<IGetDimEducationResponse, IDimEducationNotFoundResponse>(new
                    {
                        EducationId = msg.EducationId.Value
                    });
                if (response.Is(out Response<IDimEducationNotFoundResponse>? _))
                {
                    _logger.LogWarning("Domain: Validation Failed. Education ID {Id} does not exist.", msg.EducationId);
                    return;
                }
            }
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
            if (msg.EducationId.HasValue)
            {
                var response =
                    await _educationClient.GetResponse<IGetDimEducationResponse, IDimEducationNotFoundResponse>(new
                    {
                        EducationId = msg.EducationId.Value
                    });
                if (response.Is(out Response<IDimEducationNotFoundResponse>? _))
                {
                    _logger.LogWarning("Domain: Validation Failed. Education ID {Id} does not exist.", msg.EducationId);
                    return;
                }
            }
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
        var msg = context.Message;
        if (msg.EducationId.HasValue)
        {
            var response =
                await _educationClient.GetResponse<IGetDimEducationResponse, IDimEducationNotFoundResponse>(new
                {
                    EducationId = msg.EducationId.Value
                });
            if (response.Is(out Response<IDimEducationNotFoundResponse>? _))
            {
                _logger.LogWarning("Domain: Validation Failed. Education ID {Id} does not exist.", msg.EducationId);
                return;
            }
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
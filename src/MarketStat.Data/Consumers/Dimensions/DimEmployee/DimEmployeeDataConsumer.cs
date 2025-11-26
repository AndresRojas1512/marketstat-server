using MarketStat.Common.Exceptions;
using MarketStat.Common.Validators.Dimensions;
using MarketStat.Contracts.Dimensions.DimEmployee;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimEmployee;

public class DimEmployeeDataConsumer :
    IConsumer<IPersistDimEmployeeCommand>,
    IConsumer<IPersistDimEmployeeUpdateCommand>,
    IConsumer<IPersistDimEmployeePartialUpdateCommand>,
    IConsumer<IPersistDimEmployeeDeleteCommand>
{
    private readonly IDimEmployeeRepository _repository;
    private readonly ILogger<DimEmployeeDataConsumer> _logger;
    
    public DimEmployeeDataConsumer(IDimEmployeeRepository repository, ILogger<DimEmployeeDataConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<IPersistDimEmployeeCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Saving Employee {RefId}...", msg.EmployeeRefId);
        var employee = new Common.Core.MarketStat.Common.Core.Dimensions.DimEmployee(0, msg.EmployeeRefId,
            msg.BirthDate, msg.CareerStartDate, msg.Gender, msg.EducationId, msg.GraduationYear);
        try
        {
            await _repository.AddEmployeeAsync(employee);
            _logger.LogInformation("Data: Saved Employee ID {Id}", employee.EmployeeId);
        }
        catch (ConflictException) 
        { 
            _logger.LogWarning("Data: Duplicate Employee RefId."); 
        }
    }
    
    public async Task Consume(ConsumeContext<IPersistDimEmployeeUpdateCommand> context)
    {
        var msg = context.Message;
        var employee = new Common.Core.MarketStat.Common.Core.Dimensions.DimEmployee(msg.EmployeeId, msg.EmployeeRefId,
            msg.BirthDate, msg.CareerStartDate, msg.Gender, msg.EducationId, msg.GraduationYear);
        try 
        { 
            await _repository.UpdateEmployeeAsync(employee); 
            _logger.LogInformation("Data: Updated Employee ID {Id}", msg.EmployeeId);
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "Update Failed"); 
        }
    }
    
    public async Task Consume(ConsumeContext<IPersistDimEmployeePartialUpdateCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Patching Employee {Id}", msg.EmployeeId);
        try
        {
            var existing = await _repository.GetEmployeeByIdAsync(msg.EmployeeId);
            
            var newRefId = msg.EmployeeRefId ?? existing.EmployeeRefId;
            var newCareerStart = msg.CareerStartDate ?? existing.CareerStartDate;
            var newEducationId = msg.EducationId ?? existing.EducationId;
            var newGradYear = msg.GraduationYear ?? existing.GraduationYear;

            DimEmployeeValidator.ValidateForUpdate(existing.EmployeeId, newRefId, existing.BirthDate, newCareerStart, existing.Gender, newEducationId, newGradYear);

            existing.EmployeeRefId = newRefId;
            existing.CareerStartDate = newCareerStart;
            existing.EducationId = newEducationId;
            existing.GraduationYear = newGradYear;
            
            await _repository.UpdateEmployeeAsync(existing);
            _logger.LogInformation("Data: Patch Successful");
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "Patch Failed"); 
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimEmployeeDeleteCommand> context)
    {
        try 
        { 
            await _repository.DeleteEmployeeAsync(context.Message.EmployeeId); 
            _logger.LogInformation("Data: Deleted Employee {Id}", context.Message.EmployeeId);
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "Delete Failed"); 
        }
    }
}
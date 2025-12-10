using MarketStat.Common.Validators.Dimensions;
using MarketStat.Contracts.Dimensions.DimIndustryField;
using MarketStat.Contracts.Dimensions.DimJob;
using MassTransit;

namespace MarketStat.Domain.Consumers.Dimensions;

public class DimJobDomainConsumer :
    IConsumer<ISubmitDimJobCommand>,
    IConsumer<ISubmitDimJobUpdateCommand>,
    IConsumer<ISubmitDimJobDeleteCommand>
{
    private readonly ILogger<DimJobDomainConsumer> _logger;
    private readonly IRequestClient<IGetDimIndustryFieldRequest> _industryClient;

    public DimJobDomainConsumer(ILogger<DimJobDomainConsumer> logger, IRequestClient<IGetDimIndustryFieldRequest> industryClient)
    {
        _logger = logger;
        _industryClient = industryClient;
    }

    public async Task Consume(ConsumeContext<ISubmitDimJobCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating Job {Title}", msg.JobRoleTitle);
        try
        {
            DimJobValidator.ValidateForCreate(msg.JobRoleTitle, msg.StandardJobRoleTitle, msg.HierarchyLevelName, msg.IndustryFieldId);
            var industryResponse = await _industryClient.GetResponse<IGetDimIndustryFieldResponse, IDimIndustryFieldNotFoundResponse>(new
            {
                msg.IndustryFieldId
            });

            if (industryResponse.Is(out Response<IDimIndustryFieldNotFoundResponse>? _))
            {
                _logger.LogWarning("Domain: Validation Failed. Industry Field ID {Id} does not exist.", msg.IndustryFieldId);
                return;
            }
            await context.Publish<IPersistDimJobCommand>(new
            {
                msg.JobRoleTitle, msg.StandardJobRoleTitle, msg.HierarchyLevelName, msg.IndustryFieldId
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimJobUpdateCommand> context)
    {
        var msg = context.Message;
        try
        {
            DimJobValidator.ValidateForUpdate(msg.JobId, msg.JobRoleTitle, msg.StandardJobRoleTitle, msg.HierarchyLevelName, msg.IndustryFieldId);

            var industryResponse = await _industryClient.GetResponse<IGetDimIndustryFieldResponse, IDimIndustryFieldNotFoundResponse>(new
            {
                msg.IndustryFieldId
            });

            if (industryResponse.Is(out Response<IDimIndustryFieldNotFoundResponse>? _))
            {
                _logger.LogWarning("Domain: Update Validation Failed. Industry Field ID {Id} does not exist.", msg.IndustryFieldId);
                return;
            }

            await context.Publish<IPersistDimJobUpdateCommand>(new
            {
                msg.JobId, msg.JobRoleTitle, msg.StandardJobRoleTitle, msg.HierarchyLevelName, msg.IndustryFieldId
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Update Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimJobDeleteCommand> context)
    {
        if (context.Message.JobId <= 0) return;
        await context.Publish<IPersistDimJobDeleteCommand>(new { context.Message.JobId });
    }
}
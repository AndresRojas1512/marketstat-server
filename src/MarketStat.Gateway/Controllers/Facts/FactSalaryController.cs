using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;
using MarketStat.Contracts.Sales.Facts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Gateway.Controllers.Facts;

[ApiController]
[Route("api/v1/factsalaries")]
[Authorize]
public class FactSalaryController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<FactSalaryController> _logger;

    public FactSalaryController(IPublishEndpoint publishEndpoint, ILogger<FactSalaryController> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateFactSalary([FromBody] CreateFactSalaryDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _logger.LogInformation("Gateway: Received FactSalary submission. Publishing to RabbitMQ...");
        await _publishEndpoint.Publish<ISubmitFactSalaryCommand>(new
        {
            request.DateId,
            request.LocationId,
            request.EmployerId,
            request.JobId,
            request.EmployeeId,
            request.SalaryAmount
        });
        return Accepted();
    }
}
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;
using MarketStat.Contracts.Facts;
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
    private readonly IRequestClient<IGetFactSalaryRequest> _readClient;
    private readonly ILogger<FactSalaryController> _logger;

    public FactSalaryController(IPublishEndpoint publishEndpoint, IRequestClient<IGetFactSalaryRequest> readClient, ILogger<FactSalaryController> logger)
    {
        _publishEndpoint = publishEndpoint;
        _readClient = readClient;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateSalaryFact([FromBody] CreateFactSalaryDto request)
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

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(FactSalaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSalaryById(long id)
    {
        var response = await _readClient.GetResponse<IGetFactSalaryResponse, IFactSalaryNotFoundResponse>(new
        {
            SalaryFactId = id
        });
        if (response.Is(out Response<IGetFactSalaryResponse> success))
        {
            return Ok(new FactSalaryDto
            {
                SalaryFactId = success.Message.SalaryFactId,
                SalaryAmount = success.Message.SalaryAmount,
                DateId = success.Message.DateId,
                LocationId = success.Message.LocationId,
                EmployerId = success.Message.EmployerId,
                JobId = success.Message.JobId,
                EmployeeId = success.Message.EmployeeId
            });
        }
        return NotFound(new { Message = $"Salary {id} not found." });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSalaryFact(long id, [FromBody] UpdateFactSalaryDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid SalaryFactId." });
        }
        _logger.LogInformation("Gateway: Submitting Update for Salary {Id}", id);
        await _publishEndpoint.Publish<ISubmitFactSalaryUpdateCommand>(new
        {
            SalaryFactId = id,
            updateDto.DateId,
            updateDto.LocationId,
            updateDto.EmployerId,
            updateDto.JobId,
            updateDto.EmployeeId,
            updateDto.SalaryAmount
        });
        return Accepted();
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteSalaryFact(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid SalaryFactId." });
        }
        _logger.LogInformation("Gateway: Submitting Delete for Salary {Id}", id);
        await _publishEndpoint.Publish<ISubmitFactSalaryDeleteCommand>(new
        {
            SalaryFactId = id
        });
        return Accepted();
    }
}
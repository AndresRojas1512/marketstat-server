using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;
using MarketStat.Contracts.Dimensions.DimDate;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Gateway.Controllers.Dimensions;

[ApiController]
[Route("api/v1/dimdates")]
[Authorize]
public class DimDateController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<IGetDimDateRequest> _readClient;
    private readonly IRequestClient<IGetAllDimDatesRequest> _listClient;

    public DimDateController(IPublishEndpoint publishEndpoint, IRequestClient<IGetDimDateRequest> readClient,
        IRequestClient<IGetAllDimDatesRequest> listClient)
    {
        _publishEndpoint = publishEndpoint;
        _readClient = readClient;
        _listClient = listClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _listClient.GetResponse<IGetAllDimDatesResponse>(new { });
        return Ok(response.Message.Dates);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _readClient.GetResponse<IGetDimDateResponse, IDimDateNotFoundResponse>(new
        {
            DateId = id
        });
        if (response.Is(out Response<IGetDimDateResponse>? success))
        {
            return Ok(success.Message);
        }
        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDimDateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _publishEndpoint.Publish<ISubmitDimDateCommand>(new
        {
            dto.FullDate
        });
        return Accepted();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDimDateDto dto)
    {
        await _publishEndpoint.Publish<ISubmitDimDateUpdateCommand>(new
        {
            DateId = id,
            dto.FullDate
        });
        return Accepted();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _publishEndpoint.Publish<ISubmitDimDateDeleteCommand>(new
        {
            DateId = id
        });
        return Accepted();
    }
}
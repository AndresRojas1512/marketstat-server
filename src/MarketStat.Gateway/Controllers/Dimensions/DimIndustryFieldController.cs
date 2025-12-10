using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;
using MarketStat.Contracts.Dimensions.DimIndustryField;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Gateway.Controllers.Dimensions;

[ApiController]
[Route("api/v1/dimindustryfields")]
[Authorize]
public class DimIndustryFieldController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<IGetDimIndustryFieldRequest> _readClient;
    private readonly IRequestClient<IGetAllDimIndustryFieldsRequest> _listClient;

    public DimIndustryFieldController(
        IPublishEndpoint publishEndpoint, 
        IRequestClient<IGetDimIndustryFieldRequest> readClient,
        IRequestClient<IGetAllDimIndustryFieldsRequest> listClient)
    {
        _publishEndpoint = publishEndpoint;
        _readClient = readClient;
        _listClient = listClient;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _listClient.GetResponse<IGetAllDimIndustryFieldsResponse>(new { });
        return Ok(response.Message.IndustryFields); 
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _readClient.GetResponse<IGetDimIndustryFieldResponse, IDimIndustryFieldNotFoundResponse>(new
        {
            IndustryFieldId = id
        });
        
        if (response.Is(out Response<IGetDimIndustryFieldResponse>? success))
        {
            return Ok(success.Message);
        }
        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDimIndustryFieldDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        await _publishEndpoint.Publish<ISubmitDimIndustryFieldCommand>(new 
        { 
            dto.IndustryFieldCode, dto.IndustryFieldName 
        });
        return Accepted();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDimIndustryFieldDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _publishEndpoint.Publish<ISubmitDimIndustryFieldUpdateCommand>(new 
        { 
            IndustryFieldId = id, dto.IndustryFieldCode, dto.IndustryFieldName 
        });
        return Accepted();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _publishEndpoint.Publish<ISubmitDimIndustryFieldDeleteCommand>(new
        {
            IndustryFieldId = id
        });
        return Accepted();
    }
}
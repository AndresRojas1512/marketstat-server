using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducation;
using MarketStat.Contracts.Dimensions.DimEducation;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Gateway.Controllers.Dimensions;

public class DimEducationController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<IGetDimEducationRequest> _readClient;
    private readonly IRequestClient<IGetAllDimEducationsRequest> _listClient;

    public DimEducationController(IPublishEndpoint publishEndpoint, IRequestClient<IGetDimEducationRequest> readClient,
        IRequestClient<IGetAllDimEducationsRequest> listClient)
    {
        _publishEndpoint = publishEndpoint;
        _readClient = readClient;
        _listClient = listClient;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _listClient.GetResponse<IGetAllDimEducationsResponse>(new { });
        return Ok(response.Message.Educations); 
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _readClient.GetResponse<IGetDimEducationResponse, IDimEducationNotFoundResponse>(new
        {
            EducationId = id
        });
        if (response.Is(out Response<IGetDimEducationResponse>? success))
        {
            return Ok(success.Message);
        }
        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDimEducationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        await _publishEndpoint.Publish<ISubmitDimEducationCommand>(new 
        { 
            dto.SpecialtyName, dto.SpecialtyCode, dto.EducationLevelName 
        });
        return Accepted();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDimEducationDto dto)
    {
        await _publishEndpoint.Publish<ISubmitDimEducationUpdateCommand>(new 
        { 
            EducationId = id, dto.SpecialtyName, dto.SpecialtyCode, dto.EducationLevelName 
        });
        return Accepted();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _publishEndpoint.Publish<ISubmitDimEducationDeleteCommand>(new
        {
            EducationId = id
        });
        return Accepted();
    }
}
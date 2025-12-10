using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJob;
using MarketStat.Contracts.Dimensions.DimJob;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Gateway.Controllers.Dimensions;

[ApiController]
[Route("api/v1/dimjobs")]
[Authorize]
public class DimJobController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<IGetDimJobRequest> _readClient;
    private readonly IRequestClient<IGetAllDimJobsRequest> _listClient;
    private readonly IRequestClient<IGetStandardJobRolesRequest> _rolesClient;
    private readonly IRequestClient<IGetHierarchyLevelsRequest> _levelsClient;

    public DimJobController(
        IPublishEndpoint publishEndpoint,
        IRequestClient<IGetDimJobRequest> readClient,
        IRequestClient<IGetAllDimJobsRequest> listClient,
        IRequestClient<IGetStandardJobRolesRequest> rolesClient,
        IRequestClient<IGetHierarchyLevelsRequest> levelsClient)
    {
        _publishEndpoint = publishEndpoint;
        _readClient = readClient;
        _listClient = listClient;
        _rolesClient = rolesClient;
        _levelsClient = levelsClient;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _listClient.GetResponse<IGetAllDimJobsResponse>(new { });
        return Ok(response.Message.Jobs);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _readClient.GetResponse<IGetDimJobResponse, IDimJobNotFoundResponse>(new
        {
            JobId = id
        });
        if (response.Is(out Response<IGetDimJobResponse>? success))
            return Ok(success.Message);
        return NotFound();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDimJobDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        await _publishEndpoint.Publish<ISubmitDimJobCommand>(new
        {
            dto.JobRoleTitle,
            dto.StandardJobRoleTitle,
            dto.HierarchyLevelName,
            dto.IndustryFieldId
        });
        return Accepted();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDimJobDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        await _publishEndpoint.Publish<ISubmitDimJobUpdateCommand>(new
        {
            JobId = id,
            dto.JobRoleTitle,
            dto.StandardJobRoleTitle,
            dto.HierarchyLevelName,
            dto.IndustryFieldId
        });
        return Accepted();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _publishEndpoint.Publish<ISubmitDimJobDeleteCommand>(new
        {
            JobId = id
        });
        return Accepted();
    }
    
    [HttpGet("lookup/standard-roles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStandardJobRoles([FromQuery] int? industryFieldId)
    {
        var response = await _rolesClient.GetResponse<IGetStandardJobRolesResponse>(new
        {
            IndustryFieldId = industryFieldId
        });
        return Ok(response.Message.Roles);
    }

    [HttpGet("lookup/hierarchy-levels")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHierarchyLevels([FromQuery] int? industryFieldId, [FromQuery] string? standardJobRoleTitle)
    {
        var response = await _levelsClient.GetResponse<IGetHierarchyLevelsResponse>(new
        {
            IndustryFieldId = industryFieldId,
            StandardJobRoleTitle = standardJobRoleTitle
        });
        return Ok(response.Message.Levels);
    }
}
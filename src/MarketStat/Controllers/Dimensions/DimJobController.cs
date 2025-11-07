using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJob;
using MarketStat.Services.Dimensions.DimJobService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimjobs")]
[Authorize]
public class DimJobController : ControllerBase
{
    private readonly IDimJobService _dimJobService;
    private readonly IMapper _mapper;

    public DimJobController(IDimJobService dimJobService, IMapper mapper)
    {
        _dimJobService = dimJobService;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<DimJobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DimJobDto>>> GetAll()
    {
        var jobs = await _dimJobService.GetAllJobsAsync();
        return Ok(_mapper.Map<IEnumerable<DimJobDto>>(jobs));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DimJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimJobDto>> GetById(int id)
    {
        var job = await _dimJobService.GetJobByIdAsync(id);
        return Ok(_mapper.Map<DimJobDto>(job));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DimJobDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DimJobDto>> CreateJob([FromBody] CreateDimJobDto createDimJobDto)
    {
        var created = await _dimJobService.CreateJobAsync(createDimJobDto.JobRoleTitle,
            createDimJobDto.StandardJobRoleTitle, createDimJobDto.HierarchyLevelName, createDimJobDto.IndustryFieldId);
        var dto = _mapper.Map<DimJobDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.JobId }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateDimJobDto updateDimJobDto)
    {
        await _dimJobService.UpdateJobAsync(id, updateDimJobDto.JobRoleTitle, updateDimJobDto.StandardJobRoleTitle,
            updateDimJobDto.HierarchyLevelName, updateDimJobDto.IndustryFieldId);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteJob(int id)
    {
        await _dimJobService.DeleteJobAsync(id);
        return NoContent();
    }

    [HttpGet("lookup/standard-roles")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetStandardJobRoles([FromQuery] int? industryFieldId)
    {
        var roles = await _dimJobService.GetDistinctStandardJobRolesAsync(industryFieldId);
        return Ok(roles);
    }

    [HttpGet("lookup/hierarchy-levels")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetHierarchyLevels([FromQuery] int? industryFieldId,
        [FromQuery] string? standardJobRoleTitle)
    {
        var levels = await _dimJobService.GetDistinctHierarchyLevelsAsync(industryFieldId, standardJobRoleTitle);
        return Ok(levels);
    }
}
using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJobRole;
using MarketStat.Services.Dimensions.DimJobRoleService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimjobroles")]
public class DimJobRoleController : ControllerBase
{
    private readonly IDimJobRoleService _dimJobRoleService;
    private readonly IMapper _mapper;

    public DimJobRoleController(IDimJobRoleService dimJobRoleService, IMapper mapper)
    {
        _dimJobRoleService = dimJobRoleService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Returns all job roles.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimJobRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimJobRoleDto>>> GetAll()
    {
        var list = await _dimJobRoleService.GetAllJobRolesAsync();
        var dtos = _mapper.Map<IEnumerable<DimJobRoleDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a single job role by ID.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimJobRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimJobRoleDto>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid JobRoleId." });
        }
        var job = await _dimJobRoleService.GetJobRoleByIdAsync(id);
        var dto = _mapper.Map<DimJobRoleDto>(job);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new job role.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimJobRoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DimJobRoleDto>> PostJobRole([FromBody] CreateDimJobRoleDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var created = await _dimJobRoleService.CreateJobRoleAsync(createDto.JobRoleTitle,
            createDto.StandardJobRoleId, createDto.HierarchyLevelId);
        var dto = _mapper.Map<DimJobRoleDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.JobRoleId }, dto);
    }
    
    /// <summary>
    /// Updates a job role.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutJobRole(int id, [FromBody] UpdateDimJobRoleDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0) 
        {
            return BadRequest(new { Message = "Invalid JobRoleId." });
        }
        await _dimJobRoleService.UpdateJobRoleAsync(
            id,
            updateDto.JobRoleTitle,
            updateDto.StandardJobRoleId,
            updateDto.HierarchyLevelId);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a job role.
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] 
    public async Task<IActionResult> DeleteJobRole(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid JobRoleId." });
        }
        await _dimJobRoleService.DeleteJobRoleAsync(id);
        return NoContent();
    }
}
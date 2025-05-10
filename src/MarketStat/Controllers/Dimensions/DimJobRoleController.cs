using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJobRole;
using MarketStat.Services.Dimensions.DimJobRoleService;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimJobRoleDto>> GetById(int id)
    {
        var job = await _dimJobRoleService.GetJobRoleByIdAsync(id);
        var dto = _mapper.Map<DimJobRoleDto>(job);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new job role.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimJobRoleDto>> PostJobRole([FromBody] CreateDimJobRoleDto createDto)
    {
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutJobRole(int id, [FromBody] UpdateDimJobRoleDto updateDto)
    {
        await _dimJobRoleService.UpdateJobRoleAsync(id, updateDto.JobRoleTitle, updateDto.StandardJobRoleId,
            updateDto.HierarchyLevelId);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a job role.
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteJobRole(int id)
    {
        await _dimJobRoleService.DeleteJobRoleAsync(id);
        return NoContent();
    }
}
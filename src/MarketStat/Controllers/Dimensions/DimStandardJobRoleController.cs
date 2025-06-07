using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimStandardJobRole;
using MarketStat.Services.Dimensions.DimStandardJobRoleService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimstandardjobroles")]
public class DimStandardJobRoleController : ControllerBase
{
    private readonly IDimStandardJobRoleService _dimStandardJobRoleService;
    private readonly IMapper _mapper;

    public DimStandardJobRoleController(IDimStandardJobRoleService dimStandardJobRoleService, IMapper mapper)
    {
        _dimStandardJobRoleService = dimStandardJobRoleService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Return all standard job roles.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimStandardJobRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimStandardJobRoleDto>>> GetAll()
    {
        var list = await _dimStandardJobRoleService.GetAllStandardJobRolesAsync();
        var dtos = _mapper.Map<IEnumerable<DimStandardJobRoleDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a single standard job role by ID.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimStandardJobRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimStandardJobRoleDto>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid StandardJobRoleId." });
        }
        var job = await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(id);
        var dto = _mapper.Map<DimStandardJobRoleDto>(job);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new standard job role.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimStandardJobRoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DimStandardJobRoleDto>> PostStandardJobRole([FromBody] CreateDimStandardJobRoleDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _dimStandardJobRoleService.CreateStandardJobRoleAsync(
            createDto.StandardJobRoleCode,
            createDto.StandardJobRoleTitle,
            createDto.IndustryFieldId
        );
        var dto = _mapper.Map<DimStandardJobRoleDto>(created);

        return CreatedAtAction(nameof(GetById), new { id = dto.StandardJobRoleId }, dto);
    }
    
    /// <summary>
    /// Updates an existing standard job role.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutStandardJobRole(int id, [FromBody] UpdateDimStandardJobRoleDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0) 
        {
            return BadRequest(new { Message = "Invalid StandardJobRoleId." });
        }

        await _dimStandardJobRoleService.UpdateStandardJobRoleAsync(
            id,
            updateDto.StandardJobRoleCode,
            updateDto.StandardJobRoleTitle,
            updateDto.IndustryFieldId
        );

        return NoContent();
    }
    
    /// <summary>
    /// Deletes a standard job role.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DeleteStandardJobRole(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid StandardJobRoleId." });
        }
        await _dimStandardJobRoleService.DeleteStandardJobRoleAsync(id);
        return NoContent();
    }
    
    [HttpGet("byindustry/{industryFieldId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimStandardJobRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimStandardJobRoleDto>>> GetByIndustryId(int industryFieldId)
    {
        if (industryFieldId <= 0)
        {
            return BadRequest(new { Message = "IndustryFieldId must be a positive integer." });
        }
        var roles = await _dimStandardJobRoleService.GetStandardJobRolesByIndustryAsync(industryFieldId);
        var dtos = _mapper.Map<IEnumerable<DimStandardJobRoleDto>>(roles);
        return Ok(dtos);
    }
}
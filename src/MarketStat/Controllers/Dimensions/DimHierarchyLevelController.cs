using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimHierarchyLevel;
using MarketStat.Services.Dimensions.DimHierarchyLevelService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimhierarchylevels")]
public class DimHierarchyLevelController : ControllerBase
{
    private readonly IDimHierarchyLevelService _dimHierarchyLevelService;
    private readonly IMapper _mapper;

    public DimHierarchyLevelController(IDimHierarchyLevelService dimHierarchyLevelService, IMapper mapper)
    {
        _dimHierarchyLevelService = dimHierarchyLevelService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Returns all hierarchy levels.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimHierarchyLevelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimHierarchyLevelDto>>> GetAll()
    {
        var list = await _dimHierarchyLevelService.GetAllHierarchyLevelsAsync();
        var dtos = _mapper.Map<IEnumerable<DimHierarchyLevelDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a single hierarchy level.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimHierarchyLevelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimHierarchyLevelDto>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid HierarchyLevelId." });
        }
        var level = await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(id);
        var dto = _mapper.Map<DimHierarchyLevelDto>(level);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new hierarchy level.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimHierarchyLevelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DimHierarchyLevelDto>> PostHierarchyLevel([FromBody] CreateDimHierarchyLevelDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _dimHierarchyLevelService.CreateHierarchyLevelAsync(createDto.HierarchyLevelCode, createDto.HierarchyLevelName);
        var dto = _mapper.Map<DimHierarchyLevelDto>(created);

        return CreatedAtAction(nameof(GetById), new { id = dto.HierarchyLevelId }, dto);
    }
    
    /// <summary>
    /// Updates an existing hierarchy level.
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
    public async Task<IActionResult> PutHierarchyLevel(int id, [FromBody] UpdateDimHierarchyLevelDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0) 
        {
            return BadRequest(new { Message = "Invalid HierarchyLevelId." });
        }

        await _dimHierarchyLevelService.UpdateHierarchyLevelAsync(id, updateDto.HierarchyLevelCode, updateDto.HierarchyLevelName);

        return NoContent();
    }
    
    /// <summary>
    /// Deletes a hierarchy level.
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
    public async Task<IActionResult> DeleteHierarchyLevel(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid HierarchyLevelId." });
        }
        await _dimHierarchyLevelService.DeleteHierarchyLevelAsync(id);
        return NoContent();
    }
}
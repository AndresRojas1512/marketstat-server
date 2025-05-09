using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimHierarchyLevel;
using MarketStat.Services.Dimensions.DimHierarchyLevelService;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimHierarchyLevelDto>> GetById(int id)
    {
        var level = await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(id);
        var dto = _mapper.Map<DimHierarchyLevelDto>(level);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new hierarchy level.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimHierarchyLevelDto>> PostHierarchyLevel(
        [FromBody] CreateDimHierarchyLevelDto createDto)
    {
        var created = await _dimHierarchyLevelService.CreateHierarchyLevelAsync(createDto.HierarchyLevelName);
        var dto = _mapper.Map<DimHierarchyLevelDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.HierarchyLevelId }, dto);
    }
    
    /// <summary>
    /// Updates an existing hierarchy level.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutHierarchyLevel(int id, [FromBody] UpdateDimHierarchyLevelDto updateDto)
    {
        await _dimHierarchyLevelService.UpdateHierarchyLevelAsync(id, updateDto.HierarchyLevelName);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a hierarchy level.
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHierarchyLevel(int id)
    {
        await _dimHierarchyLevelService.DeleteHierarchyLevelAsync(id);
        return NoContent();
    }
}
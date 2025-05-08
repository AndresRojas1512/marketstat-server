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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimHierarchyLevelDto>>> GetAll()
    {
        var list = await _dimHierarchyLevelService.GetAllHierarchyLevelsAsync();
        var dtos = _mapper.Map<IEnumerable<DimHierarchyLevelDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimHierarchyLevelDto>> GetById(int id)
    {
        try
        {
            var level = await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(id);
            var dto = _mapper.Map<DimHierarchyLevelDto>(level);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimHierarchyLevelDto>> PostHierarchyLevel(
        [FromBody] CreateDimHierarchyLevelDto createDto)
    {
        try
        {
            var created = await _dimHierarchyLevelService.CreateHierarchyLevelAsync(createDto.HierarchyLevelName);
            var dto = _mapper.Map<DimHierarchyLevelDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = dto.HierarchyLevelId }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutHierarchyLevel(int id, [FromBody] UpdateDimHierarchyLevelDto updateDto)
    {
        try
        {
            await _dimHierarchyLevelService.UpdateHierarchyLevelAsync(id, updateDto.HierarchyLevelName);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHierarchyLevel(int id)
    {
        try
        {
            await _dimHierarchyLevelService.DeleteHierarchyLevelAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
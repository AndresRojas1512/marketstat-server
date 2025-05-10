using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimStandardJobRoleHierarchy;
using MarketStat.Services.Dimensions.DimStandardJobRoleHierarchyService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimstandardjobrolehierarchies")]
public class DimStandardJobRoleHierarchyController : ControllerBase
{
    private readonly IDimStandardJobRoleHierarchyService _dimStandardJobRoleHierarchyService;
    private readonly IMapper _mapper;

    public DimStandardJobRoleHierarchyController(IDimStandardJobRoleHierarchyService dimStandardJobRoleHierarchyService,
        IMapper mapper)
    {
        _dimStandardJobRoleHierarchyService = dimStandardJobRoleHierarchyService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Returns all StandardJobRole-HierarchyLevel links.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DimStandardJobRoleHierarchyDto>>> GetAll()
    {
        var list = await _dimStandardJobRoleHierarchyService.GetAllStandardJobRoleHierarchiesAsync();
        var dtos = _mapper.Map<IEnumerable<DimStandardJobRoleHierarchyDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a StandardJobRole-HierarchyLevel link.
    /// </summary>
    /// <param name="jobRoleId"></param>
    /// <param name="levelId"></param>
    [HttpGet("{jobRoleId:int}/{levelId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimStandardJobRoleHierarchyDto>> GetByJobRoleIdLevelId(int jobRoleId, int levelId)
    {
        try
        {
            var link = await _dimStandardJobRoleHierarchyService.GetStandardJobRoleHierarchyAsync(jobRoleId, levelId);
            var dto = _mapper.Map<DimStandardJobRoleHierarchyDto>(link);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
    
    /// <summary>
    /// Get hierarchy levels by standard job role.
    /// </summary>
    /// <param name="jobRoleId"></param>
    [HttpGet("byjobrole/{jobRoleId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DimStandardJobRoleHierarchyDto>>> GetLevelsByJobRole(int jobRoleId)
    {
        var list = await _dimStandardJobRoleHierarchyService.GetLevelsByJobRoleIdAsync(jobRoleId);
        var dtos = _mapper.Map<IEnumerable<DimStandardJobRoleHierarchyDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Get standard job roles by hierarchy level.
    /// </summary>
    /// <param name="levelId"></param>
    [HttpGet("bylevel/{levelId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DimStandardJobRoleHierarchyDto>>> GetJobRolesByLevel(int levelId)
    {
        var list = await _dimStandardJobRoleHierarchyService.GetJobRolesByLevelIdAsync(levelId);
        var dtos = _mapper.Map<IEnumerable<DimStandardJobRoleHierarchyDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Creates a StandardJobRole-HierarchyLevel link.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimStandardJobRoleHierarchyDto>> PostStandardJobRoleHierarchy(
        [FromBody] CreateDimStandardJobRoleHierarchyDto createDto)
    {
        var created =
            await _dimStandardJobRoleHierarchyService.CreateStandardJobRoleHierarchy(createDto.StandardJobRoleId,
                createDto.HierarchyLevelId);
        var dto = _mapper.Map<DimStandardJobRoleHierarchyDto>(created);
        return CreatedAtAction(nameof(GetByJobRoleIdLevelId),
            new { jobRoleId = dto.StandardJobRoleId, levelId = dto.HierarchyLevelId }, dto);
    }
    
    /// <summary>
    /// Deletes a StandardJobRole-HierarchyLevel link.
    /// </summary>
    /// <param name="jobRoleId"></param>
    /// <param name="levelId"></param>
    [HttpDelete("{jobRoleId:int}/{levelId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStandardJobRoleHierarchy(int jobRoleId, int levelId)
    {
        await _dimStandardJobRoleHierarchyService.DeleteStandardJobRoleHierarchyAsync(jobRoleId, levelId);
        return NoContent();
    }
}
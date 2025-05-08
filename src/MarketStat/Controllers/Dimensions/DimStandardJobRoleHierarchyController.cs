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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimStandardJobRoleHierarchyDto>>> GetAll()
    {
        var list = await _dimStandardJobRoleHierarchyService.GetAllStandardJobRoleHierarchiesAsync();
        var dtos = _mapper.Map<IEnumerable<DimStandardJobRoleHierarchyDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{jobRoleId:int}/{levelId:int}")]
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

    [HttpGet("byjobrole/{jobRoleId:int}")]
    public async Task<ActionResult<IEnumerable<DimStandardJobRoleHierarchyDto>>> GetLevelsByJobRole(int jobRoleId)
    {
        try
        {
            var list = await _dimStandardJobRoleHierarchyService.GetLevelsByJobRoleIdAsync(jobRoleId);
            var dtos = _mapper.Map<IEnumerable<DimStandardJobRoleHierarchyDto>>(list);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpGet("bylevel/{levelId:int}")]
    public async Task<ActionResult<IEnumerable<DimStandardJobRoleHierarchyDto>>> GetJobRolesByLevel(int levelId)
    {
        try
        {
            var list = await _dimStandardJobRoleHierarchyService.GetJobRolesByLevelIdAsync(levelId);
            var dtos = _mapper.Map<IEnumerable<DimStandardJobRoleHierarchyDto>>(list);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimStandardJobRoleHierarchyDto>> PostStandardJobRoleHierarchy(
        [FromBody] CreateDimStandardJobRoleHierarchyDto createDto)
    {
        try
        {
            var created =
                await _dimStandardJobRoleHierarchyService.CreateStandardJobRoleHierarchy(createDto.StandardJobRoleId,
                    createDto.HierarchyLevelId);
            var dto = _mapper.Map<DimStandardJobRoleHierarchyDto>(created);
            return CreatedAtAction(nameof(GetByJobRoleIdLevelId),
                new { jobRoleId = dto.StandardJobRoleId, levelId = dto.HierarchyLevelId }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{jobRoleId:int}/{levelId:int}")]
    public async Task<IActionResult> DeleteStandardJobRoleHierarchy(int jobRoleId, int levelId)
    {
        try
        {
            await _dimStandardJobRoleHierarchyService.DeleteStandardJobRoleHierarchyAsync(jobRoleId, levelId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound( new { Message = ex.Message });
        }
    }
}
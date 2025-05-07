using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducationLevel;
using MarketStat.Services.Dimensions.DimEducationLevelService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimeducationlevels")]
public class DimEducationLevelController : ControllerBase
{
    private readonly IDimEducationLevelService _dimEducationLevelService;
    private readonly IMapper _mapper;

    public DimEducationLevelController(IDimEducationLevelService dimEducationLevelService, IMapper mapper)
    {
        _dimEducationLevelService = dimEducationLevelService;
        _mapper = mapper;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimEducationLevelDto>>> GetAll()
    {
        var list = await _dimEducationLevelService.GetAllEducationLevelsAsync();
        var dtos = _mapper.Map<IEnumerable<DimEducationLevelDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimEducationLevelDto>> GetEducationLevel(int id)
    {
        try
        {
            var level = await _dimEducationLevelService.GetEducationLevelByIdAsync(id);
            var dto = _mapper.Map<DimEducationLevelDto>(level);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimEducationLevelDto>> PostEducationLevel(CreateDimEducationLevelDto createDto)
    {
        try
        {
            var created = await _dimEducationLevelService.CreateEducationLevelAsync(createDto.EducationLevelName);
            var dto = _mapper.Map<DimEducationLevelDto>(created);
            return CreatedAtAction(nameof(GetEducationLevel), new { id = dto.EducationLevelId });
        }
        catch (Exception ex)
        {
            return BadRequest( new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> PutEducationLevel(int id, UpdateDimEducationLevelDto dto)
    {
        try
        {
            await _dimEducationLevelService.UpdateEducationLevelAsync(id, dto.EducationLevelName);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteEducationLevel(int id)
    {
        try
        {
            await _dimEducationLevelService.DeleteEducationLevelAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducation;
using MarketStat.Services.Dimensions.DimEducationService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimeducations")]
public class DimEducationController : ControllerBase
{
    private readonly IDimEducationService _dimEducationService;
    private readonly IMapper _mapper;

    public DimEducationController(IDimEducationService dimEducationService, IMapper mapper)
    {
        _dimEducationService = dimEducationService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimEducationDto>>> GetAll()
    {
        var list = await _dimEducationService.GetAllEducationsAsync();
        var dtos = _mapper.Map<IEnumerable<DimEducationDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimEducationDto>> GetById(int id)
    {
        try
        {
            var edu = await _dimEducationService.GetEducationByIdAsync(id);
            return Ok(_mapper.Map<DimEducationDto>(edu));
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimEducationDto>> CreateEducation([FromBody] CreateDimEducationDto createDto)
    {
        try
        {
            var created = await _dimEducationService.CreateEducationAsync(
                createDto.Specialty,
                createDto.SpecialtyCode,
                createDto.EducationLevelId,
                createDto.IndustryFieldId
            );
            var dto = _mapper.Map<DimEducationDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = dto.EducationId }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEducation(int id, [FromBody] UpdateDimEducationDto updateDto)
    {
        try
        {
            await _dimEducationService.UpdateEducationAsync(
                id,
                updateDto.Specialty,
                updateDto.SpecialtyCode,
                updateDto.EducationLevelId,
                updateDto.IndustryFieldId
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _dimEducationService.DeleteEducationAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
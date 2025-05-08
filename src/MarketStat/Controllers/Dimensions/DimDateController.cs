using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;
using MarketStat.Services.Dimensions.DimDateService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimdates")]
public class DimDateController : ControllerBase
{
    private readonly IDimDateService _dimDateService;
    private readonly IMapper _mapper;

    public DimDateController(IDimDateService dimDateService, IMapper mapper)
    {
        _dimDateService = dimDateService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimDateDto>>> GetAll()
    {
        var dates = await _dimDateService.GetAllDatesAsync();
        var dtos = _mapper.Map<IEnumerable<DimDateDto>>(dates);
        return Ok(dtos);
    }

    [HttpGet("id:int")]
    public async Task<ActionResult<DimDateDto>> GetById(int id)
    {
        try
        {
            var date = await _dimDateService.GetDateByIdAsync(id);
            return Ok(_mapper.Map<DimDateDto>(date));
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimDateDto>> CreateDate([FromBody] CreateDimDateDto createDto)
    {
        try
        {
            var created = await _dimDateService.CreateDateAsync(createDto.FullDate);
            var dto = _mapper.Map<DimDateDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = dto.DateId }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateDate(int id, [FromBody] UpdateDimDateDto updateDto)
    {
        try
        {
            await _dimDateService.UpdateDateAsync(id, updateDto.FullDate);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDate(int id)
    {
        try
        {
            await _dimDateService.DeleteDateAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
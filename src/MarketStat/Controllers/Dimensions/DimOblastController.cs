using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimOblast;
using MarketStat.Services.Dimensions.DimOblastService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/oblasts")]
public class DimOblastController : ControllerBase
{
    private readonly DimOblastService _dimOblastService;
    private readonly IMapper _mapper;

    public DimOblastController(DimOblastService dimOblastService, IMapper mapper)
    {
        _dimOblastService = dimOblastService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimOblastDto>>> GetOblasts()
    {
        var list = await _dimOblastService.GetAllOblastsAsync();
        var dtos = _mapper.Map<IEnumerable<DimOblastDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimOblastDto>> GetOblast(int id)
    {
        try
        {
            var oblast = await _dimOblastService.GetOblastsByFederalDistrictIdAsync(id);
            var dto = _mapper.Map<DimOblastDto>(oblast);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpGet] // TODO
    public async Task<ActionResult<DimOblastDto>> GetOblastByFederalDistrict(int districtId)
    {
        try
        {
            var oblasts = await _dimOblastService.GetOblastsByFederalDistrictIdAsync(districtId);
            var dtos = _mapper.Map<IEnumerable<DimOblastDto>>(oblasts);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return NotFound( new { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimOblastDto>> PostOblast(DimOblastDto createDto)
    {
        try
        {
            var created = await _dimOblastService.CreateOblastAsync(createDto.OblastName, createDto.DistrictId);
            var dto = _mapper.Map<DimOblastDto>(created);
            return CreatedAtAction(nameof(GetOblast), new { id = dto.OblastId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> PutOblast(int id, UpdateDimOblastDto updateDto)
    {
        try
        {
            await _dimOblastService.UpdateOblastAsync(id, updateDto.OblastName, updateDto.DistrictId);
            return NoContent();
        }
        catch (Exception e)
        {
            return NotFound(new { Message = e.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteOblast(int id)
    {
        try
        {
            await _dimOblastService.DeleteOblastAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
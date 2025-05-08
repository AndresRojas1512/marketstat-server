using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimFederalDistrict;
using MarketStat.Services.Dimensions.DimFederalDistrictService;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimfederaldistricts")]
public class DimFederalDistrictController : ControllerBase
{
    private readonly IDimFederalDistrictService _dimFederalDistrictService;
    private readonly IMapper _mapper;

    public DimFederalDistrictController(IDimFederalDistrictService dimFederalDistrictService, IMapper mapper)
    {
        _dimFederalDistrictService = dimFederalDistrictService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimFederalDistrictDto>>> GetAll()
    {
        var list = await _dimFederalDistrictService.GetAllDistrictsAsync();
        var dtos = _mapper.Map<IEnumerable<DimFederalDistrictDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimFederalDistrictDto>> GetById(int id)
    {
        try
        {
            var district = await _dimFederalDistrictService.GetDistrictByIdAsync(id);
            var dto = _mapper.Map<DimFederalDistrictDto>(district);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimFederalDistrictDto>> PostFederalDistrict(
        [FromBody] CreateDimFederalDistrictDto createDto)
    {
        try
        {
            var created = await _dimFederalDistrictService.CreateDistrictAsync(createDto.DistrictName);
            var dto = _mapper.Map<DimFederalDistrictDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = dto.DistrictId }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutFederalDistrict(int id, [FromBody] UpdateDimFederalDistrictDto updateDto)
    {
        try
        {
            await _dimFederalDistrictService.UpdateDistrictAsync(id, updateDto.DistrictName);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteById(int id)
    {
        try
        {
            await _dimFederalDistrictService.DeleteDistrictAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
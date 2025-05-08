using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimCity;
using MarketStat.Services.Dimensions.DimCityService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimcities")]
public class DimCityController : ControllerBase
{
    private readonly IDimCityService _dimCityService;
    private readonly IMapper _mapper;

    public DimCityController(IDimCityService dimCityService, IMapper mapper)
    {
        _dimCityService  = dimCityService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimCityDto>>> GetCities()
    {
        var cities = await _dimCityService.GetAllCitiesAsync();
        var dtos = _mapper.Map<IEnumerable<DimCityDto>>(cities);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimCityDto>> GetCity(int id)
    {
        try
        {
            var city = await _dimCityService.GetCityByIdAsync(id);
            var dto = _mapper.Map<DimCityDto>(city);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimCityDto>> CreateCity([FromBody] CreateDimCityDto createDto)
    {
        try
        {
            var created = await _dimCityService.CreateCityAsync(createDto.CityName, createDto.OblastId);
            var dto = _mapper.Map<DimCityDto>(created);
            return CreatedAtAction(nameof(GetCity), new { id = dto.CityId }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCity(int id, [FromBody] UpdateDimCityDto updateDto)
    {
        try
        {
            await _dimCityService.UpdateCityAsync(id, updateDto.CityName, updateDto.OblastId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCity(int id)
    {
        try
        {
            await _dimCityService.DeleteCityAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
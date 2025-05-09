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
    
    /// <summary>
    /// Returns all cities.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DimCityDto>>> GetAll()
    {
        var cities = await _dimCityService.GetAllCitiesAsync();
        var dtos = _mapper.Map<IEnumerable<DimCityDto>>(cities);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a single city by ID.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimCityDto>> GetById(int id)
    {
        var city = await _dimCityService.GetCityByIdAsync(id);
        var dto = _mapper.Map<DimCityDto>(city);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new city.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimCityDto>> CreateCity([FromBody] CreateDimCityDto createDto)
    {
        var city = await _dimCityService.CreateCityAsync(createDto.CityName, createDto.OblastId);
        var dto = _mapper.Map<DimCityDto>(city);
        return CreatedAtAction(nameof(GetById), new { id = dto.CityId }, dto);
    }
    
    /// <summary>
    /// Updates an existing city.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCity(int id, [FromBody] UpdateDimCityDto updateDto)
    {

        await _dimCityService.UpdateCityAsync(id, updateDto.CityName, updateDto.OblastId);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a city.
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCity(int id)
    {
        await _dimCityService.DeleteCityAsync(id);
        return NoContent();
    }
}
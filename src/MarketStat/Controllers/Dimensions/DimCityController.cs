using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimCity;
using MarketStat.Services.Dimensions.DimCityService;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimCityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimCityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimCityDto>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid CityId." });
        }
        var city = await _dimCityService.GetCityByIdAsync(id);
        var dto = _mapper.Map<DimCityDto>(city);
        return Ok(dto);
    }
    
    /// <summary>
    /// Gets cities filtered by Oblast ID. (Publicly accessible for cascading dropdowns)
    /// </summary>
    /// <param name="oblastId">The ID of the oblast.</param>
    [HttpGet("byoblast/{oblastId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimCityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimCityDto>>> GetCitiesByOblastId(int oblastId)
    {
        if (oblastId <= 0)
        {
            return BadRequest(new { Message = "Invalid OblastId." });
        }
        var citiesDomain = await _dimCityService.GetCitiesByOblastIdAsync(oblastId);
        var dtos = _mapper.Map<IEnumerable<DimCityDto>>(citiesDomain);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Creates a new city.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimCityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DimCityDto>> CreateCity([FromBody] CreateDimCityDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
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
    // [Authorize(Roles = "EtlUser")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCity(int id, [FromBody] UpdateDimCityDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid CityId." });
        }
        await _dimCityService.UpdateCityAsync(id, updateDto.CityName, updateDto.OblastId);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a city.
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id:int}")]
    // [Authorize(Roles = "EtlUser")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCity(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid CityId." });
        }
        await _dimCityService.DeleteCityAsync(id);
        return NoContent();
    }
}
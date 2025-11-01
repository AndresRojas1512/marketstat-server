using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimLocation;
using MarketStat.Services.Dimencions.DimLocationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimlocations")]
[Authorize]
public class DimLocationController : ControllerBase
{
    private readonly IDimLocationService _dimLocationService;
    private readonly IMapper _mapper;

    public DimLocationController(IDimLocationService dimLocationService, IMapper mapper)
    {
        _dimLocationService = dimLocationService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DimLocationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DimLocationDto>>> GetAll()
    {
        var location = await _dimLocationService.GetAllLocationsAsync();
        var dtos = _mapper.Map<IEnumerable<DimLocationDto>>(location);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DimLocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimLocationDto>> GetById(int id)
    {
        var location = await _dimLocationService.GetLocationByIdAsync(id);
        var dto = _mapper.Map<DimLocationDto>(location);
        return Ok(dto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DimLocationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DimLocationDto>> CreateLocation([FromBody] CreateDimLocationDto createDimLocationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _dimLocationService.CreateLocationAsync(createDimLocationDto.CityName,
            createDimLocationDto.OblastName, createDimLocationDto.DistrictName);
        var dto = _mapper.Map<DimLocationDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.LocationId }, dto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateDimLocationDto updateDimLocationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _dimLocationService.UpdateLocationAsync(id, updateDimLocationDto.CityName,
            updateDimLocationDto.OblastName, updateDimLocationDto.DistrictName);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        await _dimLocationService.DeleteLocationAsync(id);
        return NoContent();
    }
}
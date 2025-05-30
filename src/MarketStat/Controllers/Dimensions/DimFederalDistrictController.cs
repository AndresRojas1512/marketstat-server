using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimFederalDistrict;
using MarketStat.Services.Dimensions.DimFederalDistrictService;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.AspNetCore.Authorization;
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
    
    /// <summary>
    /// Returns all federal districts.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimFederalDistrictDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimFederalDistrictDto>>> GetAll()
    {
        var list = await _dimFederalDistrictService.GetAllDistrictsAsync();
        var dtos = _mapper.Map<IEnumerable<DimFederalDistrictDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a single federal district by ID.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimFederalDistrictDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimFederalDistrictDto>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid DistrictId." });
        }
        var district = await _dimFederalDistrictService.GetDistrictByIdAsync(id);
        var dto = _mapper.Map<DimFederalDistrictDto>(district);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new federal district
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimFederalDistrictDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DimFederalDistrictDto>> PostFederalDistrict(
        [FromBody] CreateDimFederalDistrictDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var created = await _dimFederalDistrictService.CreateDistrictAsync(createDto.DistrictName);
        var dto = _mapper.Map<DimFederalDistrictDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.DistrictId }, dto);
    }
    
    /// <summary>
    /// Updates an existing federal district
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutFederalDistrict(int id, [FromBody] UpdateDimFederalDistrictDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0) 
        {
            return BadRequest(new { Message = "Invalid DistrictId." });
        }
        await _dimFederalDistrictService.UpdateDistrictAsync(id, updateDto.DistrictName);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a federal district
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid DistrictId." });
        }
        await _dimFederalDistrictService.DeleteDistrictAsync(id);
        return NoContent();
    }
}
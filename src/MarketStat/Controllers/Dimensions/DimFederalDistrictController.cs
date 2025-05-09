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
    
    /// <summary>
    /// Returns all federal districts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimFederalDistrictDto>> GetById(int id)
    {
        var district = await _dimFederalDistrictService.GetDistrictByIdAsync(id);
        var dto = _mapper.Map<DimFederalDistrictDto>(district);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new federal district
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimFederalDistrictDto>> PostFederalDistrict(
        [FromBody] CreateDimFederalDistrictDto createDto)
    {
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutFederalDistrict(int id, [FromBody] UpdateDimFederalDistrictDto updateDto)
    {
        await _dimFederalDistrictService.UpdateDistrictAsync(id, updateDto.DistrictName);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a federal district
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteById(int id)
    {
        await _dimFederalDistrictService.DeleteDistrictAsync(id);
        return NoContent();
    }
}
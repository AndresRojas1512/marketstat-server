using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimOblast;
using MarketStat.Services.Dimensions.DimOblastService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/oblasts")]
public class DimOblastController : ControllerBase
{
    private readonly IDimOblastService _dimOblastService;
    private readonly IMapper _mapper;

    public DimOblastController(DimOblastService dimOblastService, IMapper mapper)
    {
        _dimOblastService = dimOblastService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Returns all oblasts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DimOblastDto>>> GetAll()
    {
        var list = await _dimOblastService.GetAllOblastsAsync();
        var dtos = _mapper.Map<IEnumerable<DimOblastDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns an oblast by ID.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimOblastDto>> GetById(int id)
    {
        var oblast = await _dimOblastService.GetOblastsByFederalDistrictIdAsync(id);
        var dto = _mapper.Map<DimOblastDto>(oblast);
        return Ok(dto);
    }
    
    /// <summary>
    /// Returns oblasts by federal district.
    /// </summary>
    /// <param name="districtId"></param>
    /// <returns></returns>
    [HttpGet("bydistrict/{districtId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DimOblastDto>>> GetOblastsByFederalDistrict(int districtId)
    {
        var list = await _dimOblastService.GetOblastsByFederalDistrictIdAsync(districtId);
        var dtos = _mapper.Map<IEnumerable<DimOblastDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Creates a new oblast.
    /// </summary>
    /// <param name="createDto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimOblastDto>> PostOblast([FromBody] CreateDimOblastDto createDto)
    {
        var created = await _dimOblastService.CreateOblastAsync(createDto.OblastName, createDto.DistrictId);
        var dto = _mapper.Map<DimOblastDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.OblastId }, dto);
    }
    
    /// <summary>
    /// Updates an existing oblast.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> PutOblast(int id, UpdateDimOblastDto updateDto)
    {
        await _dimOblastService.UpdateOblastAsync(id, updateDto.OblastName, updateDto.DistrictId);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes an oblast.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
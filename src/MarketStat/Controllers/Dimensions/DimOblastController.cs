using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimOblast;
using MarketStat.Services.Dimensions.DimOblastService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimoblasts")]
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
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimOblastDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimOblastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimOblastDto>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid OblastId." });
        }
        var oblast = await _dimOblastService.GetOblastByIdAsync(id);
        var dto = _mapper.Map<DimOblastDto>(oblast);
        return Ok(dto);
    }
    
    /// <summary>
    /// Returns oblasts by federal district.
    /// </summary>
    /// <param name="districtId"></param>
    /// <returns></returns>
    [HttpGet("bydistrict/{districtId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimOblastDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimOblastDto>>> GetOblastsByFederalDistrict(int districtId)
    {
        if (districtId <= 0)
        {
            return BadRequest(new { Message = "Invalid DistrictId." });
        }
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
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimOblastDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] 
    public async Task<ActionResult<DimOblastDto>> PostOblast([FromBody] CreateDimOblastDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
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
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> PutOblast(int id, UpdateDimOblastDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0) 
        {
            return BadRequest(new { Message = "Invalid OblastId." });
        }
        await _dimOblastService.UpdateOblastAsync(id, updateDto.OblastName, updateDto.DistrictId);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes an oblast.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] 
    public async Task<ActionResult> DeleteOblast(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid OblastId." });
        }
        await _dimOblastService.DeleteOblastAsync(id);
        return NoContent();
    }
}
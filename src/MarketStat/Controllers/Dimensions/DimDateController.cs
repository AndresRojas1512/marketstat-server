using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;
using MarketStat.Services.Dimensions.DimDateService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimdates")]
public class DimDateController : ControllerBase
{
    private readonly IDimDateService _dimDateService;
    private readonly IMapper _mapper;

    public DimDateController(IDimDateService dimDateService, IMapper mapper)
    {
        _dimDateService = dimDateService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Returns all dates
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimDateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimDateDto>>> GetAll()
    {
        var dates = await _dimDateService.GetAllDatesAsync();
        var dtos = _mapper.Map<IEnumerable<DimDateDto>>(dates);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a single date by ID.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimDateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimDateDto>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid DateId." });
        }
        var date = await _dimDateService.GetDateByIdAsync(id);
        var dto = _mapper.Map<DimDateDto>(date);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new date
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimDateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimDateDto>> CreateDate([FromBody] CreateDimDateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var created = await _dimDateService.CreateDateAsync(createDto.FullDate);
        var dto = _mapper.Map<DimDateDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.DateId }, dto);
    }
    
    /// <summary>
    /// Updates an existing date.
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
    public async Task<IActionResult> UpdateDate(int id, [FromBody] UpdateDimDateDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid DateId." });
        }
        await _dimDateService.UpdateDateAsync(id, updateDto.FullDate);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a date.
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
    public async Task<IActionResult> DeleteDate(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid DateId." });
        }
        await _dimDateService.DeleteDateAsync(id);
        return NoContent();
    }
}
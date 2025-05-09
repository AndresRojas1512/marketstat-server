using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;
using MarketStat.Services.Dimensions.DimDateService;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimDateDto>> GetById(int id)
    {
        var date = await _dimDateService.GetDateByIdAsync(id);
        var dto = _mapper.Map<DimDateDto>(date);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new date
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimDateDto>> CreateDate([FromBody] CreateDimDateDto createDto)
    {
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDate(int id, [FromBody] UpdateDimDateDto updateDto)
    {
        await _dimDateService.UpdateDateAsync(id, updateDto.FullDate);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a date.
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDate(int id)
    {
        await _dimDateService.DeleteDateAsync(id);
        return NoContent();
    }
}
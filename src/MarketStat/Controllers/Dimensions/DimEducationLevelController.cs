using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducationLevel;
using MarketStat.Services.Dimensions.DimEducationLevelService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimeducationlevels")]
public class DimEducationLevelController : ControllerBase
{
    private readonly IDimEducationLevelService _dimEducationLevelService;
    private readonly IMapper _mapper;

    public DimEducationLevelController(IDimEducationLevelService dimEducationLevelService, IMapper mapper)
    {
        _dimEducationLevelService = dimEducationLevelService;
        _mapper = mapper;
    }
    /// <summary>
    /// Returns all education levels.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimEducationLevelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimEducationLevelDto>>> GetAll()
    {
        var list = await _dimEducationLevelService.GetAllEducationLevelsAsync();
        var dtos = _mapper.Map<IEnumerable<DimEducationLevelDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a single education level by ID.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimEducationLevelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimEducationLevelDto>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid EducationLevelId." });
        }
        var level = await _dimEducationLevelService.GetEducationLevelByIdAsync(id);
        var dto = _mapper.Map<DimEducationLevelDto>(level);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new education level.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimEducationLevelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] 
    public async Task<ActionResult<DimEducationLevelDto>> PostEducationLevel([FromBody] CreateDimEducationLevelDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var created = await _dimEducationLevelService.CreateEducationLevelAsync(createDto.EducationLevelName);
        var dto = _mapper.Map<DimEducationLevelDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.EducationLevelId }, dto);
    }
    
    /// <summary>
    /// Updates an existing education level.
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
    public async Task<IActionResult> PutEducationLevel(int id, [FromBody] UpdateDimEducationLevelDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0) 
        {
            return BadRequest(new { Message = "Invalid EducationLevelId." });
        }
        await _dimEducationLevelService.UpdateEducationLevelAsync(id, updateDto.EducationLevelName);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes an education level.
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
    public async Task<IActionResult> DeleteEducationLevel(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid EducationLevelId." });
        }
        await _dimEducationLevelService.DeleteEducationLevelAsync(id);
        return NoContent();
    }
}
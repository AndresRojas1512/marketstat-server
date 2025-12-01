namespace MarketStat.Controllers.Dimensions;

using AutoMapper;
using MarketStat.Common.Dto.Dimensions.DimIndustryField;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/dimindustryfields")]
[Authorize]
public class DimIndustryFieldController : ControllerBase
{
    private readonly IDimIndustryFieldService _dimIndustryFieldService;
    private readonly IMapper _mapper;

    public DimIndustryFieldController(IDimIndustryFieldService dimIndustryFieldService, IMapper mapper)
    {
        _dimIndustryFieldService = dimIndustryFieldService;
        _mapper = mapper;
    }

    /// <summary>
    /// Returns all industry fields.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<DimIndustryFieldDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimIndustryFieldDto>>> GetAll()
    {
        var list = await _dimIndustryFieldService.GetAllIndustryFieldsAsync().ConfigureAwait(false);
        var dtos = _mapper.Map<IEnumerable<DimIndustryFieldDto>>(list);
        return Ok(dtos);
    }

    /// <summary>
    /// Returns a single industry field.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DimIndustryFieldDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimIndustryFieldDto>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid IndustryFieldId." });
        }

        var field = await _dimIndustryFieldService.GetIndustryFieldByIdAsync(id).ConfigureAwait(false);
        var dto = _mapper.Map<DimIndustryFieldDto>(field);
        return Ok(dto);
    }

    /// <summary>
    /// Created a new industry field.
    /// </summary>
    /// <param name="createDto"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DimIndustryFieldDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DimIndustryFieldDto>> PostIndustryField([FromBody] CreateDimIndustryFieldDto createDto)
    {
        ArgumentNullException.ThrowIfNull(createDto);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _dimIndustryFieldService.CreateIndustryFieldAsync(
            createDto.IndustryFieldCode,
            createDto.IndustryFieldName).ConfigureAwait(false);
        var dto = _mapper.Map<DimIndustryFieldDto>(created);

        return CreatedAtAction(nameof(GetById), new { id = dto.IndustryFieldId }, dto);
    }

    /// <summary>
    /// Updates an existing industry field.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutIndustryField(int id, [FromBody] UpdateDimIndustryFieldDto updateDto)
    {
        ArgumentNullException.ThrowIfNull(updateDto);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid IndustryFieldId." });
        }

        await _dimIndustryFieldService.UpdateIndustryFieldAsync(
            id,
            updateDto.IndustryFieldCode,
            updateDto.IndustryFieldName).ConfigureAwait(false);

        return NoContent();
    }

    /// <summary>
    /// Deletes an industry field.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteIndustryField(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid IndustryFieldId." });
        }

        await _dimIndustryFieldService.DeleteIndustryFieldAsync(id).ConfigureAwait(false);
        return NoContent();
    }
}

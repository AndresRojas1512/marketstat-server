using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimindustryfields")]
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
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimIndustryFieldDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimIndustryFieldDto>>> GetAll()
    {
        var list = await _dimIndustryFieldService.GetAllIndustryFieldsAsync();
        var dtos = _mapper.Map<IEnumerable<DimIndustryFieldDto>>(list);
        return Ok(dtos);
    }
    /// <summary>
    /// Returns a single industry field.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
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
        var field = await _dimIndustryFieldService.GetIndustryFieldByIdAsync(id);
        var dto = _mapper.Map<DimIndustryFieldDto>(field);
        return Ok(dto);
    }
    
    /// <summary>
    /// Created a new industry field.
    /// </summary>
    /// <param name="createDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimIndustryFieldDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimIndustryFieldDto>> PostIndustryField(
        [FromBody] CreateDimIndustryFieldDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var created = await _dimIndustryFieldService.CreateIndustryFieldAsync(createDto.IndustryFieldName);
        var dto = _mapper.Map<DimIndustryFieldDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = created.IndustryFieldId }, dto);
    }
    
    /// <summary>
    /// Updates an existing industry field.
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
    public async Task<IActionResult> PutIndustryField(int id, [FromBody] UpdateDimIndustryFieldDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0) 
        {
            return BadRequest(new { Message = "Invalid IndustryFieldId." });
        }
        await _dimIndustryFieldService.UpdateIndustryFieldAsync(id, updateDto.IndustryFieldName);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes an industry field.
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteIndustryField(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid IndustryFieldId." });
        }
        await _dimIndustryFieldService.DeleteIndustryFieldAsync(id);
        return NoContent();
    }
}
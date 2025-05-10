using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/industryfields")]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimIndustryFieldDto>> GetById(int id)
    {
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimIndustryFieldDto>> PostIndustryField(
        [FromBody] CreateDimIndustryFieldDto createDto)
    {
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutIndustryField(int id, [FromBody] UpdateDimIndustryFieldDto updateDto)
    {
        await _dimIndustryFieldService.UpdateIndustryFieldAsync(id, updateDto.IndustryFieldName);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes an industry field.
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteIndustryField(int id)
    {
        await _dimIndustryFieldService.DeleteIndustryFieldAsync(id);
        return NoContent();
    }
    
}
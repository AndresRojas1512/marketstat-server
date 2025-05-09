using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducation;
using MarketStat.Services.Dimensions.DimEducationService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimeducations")]
public class DimEducationController : ControllerBase
{
    private readonly IDimEducationService _dimEducationService;
    private readonly IMapper _mapper;

    public DimEducationController(IDimEducationService dimEducationService, IMapper mapper)
    {
        _dimEducationService = dimEducationService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Returns all educations.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DimEducationDto>>> GetAll()
    {
        var list = await _dimEducationService.GetAllEducationsAsync();
        var dtos = _mapper.Map<IEnumerable<DimEducationDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a single education by ID.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimEducationDto>> GetById(int id)
    {
        var education = await _dimEducationService.GetEducationByIdAsync(id);
        var dto = _mapper.Map<DimEducationDto>(education);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new education.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimEducationDto>> CreateEducation([FromBody] CreateDimEducationDto createDto)
    {
        var created = await _dimEducationService.CreateEducationAsync(
            createDto.Specialty,
            createDto.SpecialtyCode,
            createDto.EducationLevelId,
            createDto.IndustryFieldId
        );
        var dto = _mapper.Map<DimEducationDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.EducationId }, dto);
    }
    
    /// <summary>
    /// Updates an existing education.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEducation(int id, [FromBody] UpdateDimEducationDto updateDto)
    {
        await _dimEducationService.UpdateEducationAsync(
            id,
            updateDto.Specialty,
            updateDto.SpecialtyCode,
            updateDto.EducationLevelId,
            updateDto.IndustryFieldId
        );
        return NoContent();
    }
    
    /// <summary>
    /// Deletes an education.
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _dimEducationService.DeleteEducationAsync(id);
        return NoContent();
    }
}
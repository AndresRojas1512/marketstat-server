using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployeeEducation;
using MarketStat.Services.Dimensions.DimEmployeeEducationService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop.Infrastructure;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimemployeeeducations")]
public class DimEmployeeEducationController : ControllerBase
{
    private readonly IDimEmployeeEducationService _dimEmployeeEducationService;
    private readonly IMapper _mapper;

    public DimEmployeeEducationController(IDimEmployeeEducationService dimEmployeeEducationService, IMapper mapper)
    {
        _dimEmployeeEducationService = dimEmployeeEducationService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Returns all employee-education links.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DimEmployeeEducation>>> GetAll()
    {
        var list = await _dimEmployeeEducationService.GetAllEmployeeEducationsAsync();
        var dtos = _mapper.Map<IEnumerable<DimEmployeeEducationDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Get a single link by employee and education IDs.
    /// </summary>
    /// <param name="employeeId"></param>
    /// <param name="educationId"></param>
    [HttpGet("{employeeId:int}/{educationId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimEmployeeEducationDto>> GetByEmployeeIdEducationId(int employeeId, int educationId)
    {
        var link = await _dimEmployeeEducationService.GetEmployeeEducationAsync(employeeId, educationId);
        var dto = _mapper.Map<DimEmployeeEducationDto>(link);
        return Ok(dto);
    }
    
    /// <summary>
    /// Returns all educations for a given employee.
    /// </summary>
    /// <param name="employeeId"></param>
    [HttpGet("byemployee/{employeeId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DimEmployeeEducationDto>>> GetEducationsByEmployeeId(int employeeId)
    {
        var list = await _dimEmployeeEducationService.GetEducationsByEmployeeIdAsync(employeeId);
        var dtos = _mapper.Map<IEnumerable<DimEmployeeEducationDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Creates a new employee-education link.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimEmployeeEducationDto>> PostEmployeeEducation(
        [FromBody] CreateDimEmployeeEducationDto createDto)
    {
        var created = await _dimEmployeeEducationService.CreateEmployeeEducationAsync(createDto.EmployeeId,
            createDto.EducationId, createDto.GraduationYear);
        var dto = _mapper.Map<DimEmployeeEducationDto>(created);
        return CreatedAtAction(nameof(GetByEmployeeIdEducationId),
            new { employeeId = dto.EmployeeId, educationId = dto.EducationId }, dto);
    }
    
    /// <summary>
    /// Updates an existing employee-education link.
    /// </summary>
    /// <param name="employeeId"></param>
    /// <param name="educationId"></param>
    /// <param name="updateDto"></param>
    [HttpPut("{employeeId:int}/{educationId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutEmployeeEducation(int employeeId, int educationId,
        [FromBody] UpdateDimEmployeeEducationDto updateDto)
    {
            await _dimEmployeeEducationService.UpdateEmployeeEducationAsync(employeeId, educationId,
                updateDto.GraduationYear);
            return NoContent();
    }
    
    /// <summary>
    /// Deletes an employee-education link.
    /// </summary>
    /// <param name="employeeId"></param>
    /// <param name="educationId"></param>
    [HttpDelete("{employeeId:int}/{educationId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteByEducationIdEmployeeId(int employeeId, int educationId)
    {
        await _dimEmployeeEducationService.DeleteEmployeeEducationAsync(employeeId, educationId);
        return NoContent();
    }
}
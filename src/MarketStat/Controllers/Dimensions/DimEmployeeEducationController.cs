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

    [HttpGet]
    public async Task<ActionResult<List<DimEmployeeEducation>>> GetAll()
    {
        var list = await _dimEmployeeEducationService.GetAllEmployeeEducationsAsync();
        var dtos = _mapper.Map<IEnumerable<DimEmployeeEducationDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{employeeId:int}/{educationId:int}")]
    public async Task<ActionResult<DimEmployeeEducationDto>> GetByEmployeeIdEducationId(int employeeId, int educationId)
    {
        try
        {
            var link = await _dimEmployeeEducationService.GetEmployeeEducationAsync(employeeId, educationId);
            var dto = _mapper.Map<DimEmployeeEducationDto>(link);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("byemployee/{employeeId:int}")]
    public async Task<ActionResult<IEnumerable<DimEmployeeEducationDto>>> GetEducationsByEmployeeId(int employeeId)
    {
        try
        {
            var list = await _dimEmployeeEducationService.GetEducationsByEmployeeIdAsync(employeeId);
            var dtos = _mapper.Map<IEnumerable<DimEmployeeEducationDto>>(list);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimEmployeeEducationDto>> PostEmployeeEducation(
        [FromBody] CreateDimEmployeeEducationDto createDto)
    {
        try
        {
            var created = await _dimEmployeeEducationService.CreateEmployeeEducationAsync(createDto.EmployeeId,
                createDto.EducationId, createDto.GraduationYear);
            var dto = _mapper.Map<DimEmployeeEducationDto>(created);
            return CreatedAtAction(nameof(GetByEmployeeIdEducationId),
                new { employeeId = dto.EmployeeId, educationId = dto.EducationId }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{employeeId:int}/{educationId:int}")]
    public async Task<IActionResult> PutEmployeeEducation(int employeeId, int educationId,
        [FromBody] UpdateDimEmployeeEducationDto updateDto)
    {
        try
        {
            await _dimEmployeeEducationService.UpdateEmployeeEducationAsync(employeeId, educationId,
                updateDto.GraduationYear);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{employeeId:int}/{educationId:int}")]
    public async Task<IActionResult> DeleteByEducationIdEmployeeId(int employeeId, int educationId)
    {
        try
        {
            await _dimEmployeeEducationService.DeleteEmployeeEducationAsync(employeeId, educationId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
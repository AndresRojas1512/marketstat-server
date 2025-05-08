using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployee;
using MarketStat.Services.Dimensions.DimEmployeeService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimemployees")]
public class DimEmployeeController : ControllerBase
{
    private readonly IDimEmployeeService _dimEmployeeService;
    private readonly IMapper _mapper;

    public DimEmployeeController(IDimEmployeeService dimEmployeeService, IMapper mapper)
    {
        _dimEmployeeService = dimEmployeeService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimEmployeeDto>>> GetAll()
    {
        var list = await _dimEmployeeService.GetAllEmployeesAsync();
        var dtos = _mapper.Map<IEnumerable<DimEmployeeDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimEmployeeDto>> GetById(int id)
    {
        try
        {
            var employee = await _dimEmployeeService.GetEmployeeByIdAsync(id);
            var dto = _mapper.Map<DimEmployeeDto>(employee);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimEmployeeDto>> PostEmployee([FromBody] CreateDimEmployeeDto createDto)
    {
        try
        {
            var created = await _dimEmployeeService.CreateEmployeeAsync(createDto.BirthDate, createDto.CareerStartDate);
            var dto = _mapper.Map<DimEmployeeDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = dto.EmployeeId} );
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutEmployee(int id, [FromBody] UpdateDimEmployeeDto updateDto)
    {
        try
        {
            await _dimEmployeeService.UpdateEmployeeAsync(id, updateDto.BirthDate, updateDto.CareerStartDate);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        try
        {
            await _dimEmployeeService.DeleteEmployeeAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
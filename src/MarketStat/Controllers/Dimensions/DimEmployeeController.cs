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
    
    /// <summary>
    /// Returns all employees.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DimEmployeeDto>>> GetAll()
    {
        var list = await _dimEmployeeService.GetAllEmployeesAsync();
        var dtos = _mapper.Map<IEnumerable<DimEmployeeDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a single employee by ID.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimEmployeeDto>> GetById(int id)
    {
        var employee = await _dimEmployeeService.GetEmployeeByIdAsync(id);
        var dto = _mapper.Map<DimEmployeeDto>(employee);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new employee.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimEmployeeDto>> PostEmployee([FromBody] CreateDimEmployeeDto createDto)
    {
        var created = await _dimEmployeeService.CreateEmployeeAsync(createDto.BirthDate, createDto.CareerStartDate);
        var dto = _mapper.Map<DimEmployeeDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.EmployeeId}, dto );
    }
    
    /// <summary>
    /// Updates an existing employee.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutEmployee(int id, [FromBody] UpdateDimEmployeeDto updateDto)
    {
        await _dimEmployeeService.UpdateEmployeeAsync(id, updateDto.BirthDate, updateDto.CareerStartDate);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes an employee
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        await _dimEmployeeService.DeleteEmployeeAsync(id);
        return NoContent();
    }
}
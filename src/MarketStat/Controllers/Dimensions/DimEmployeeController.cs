using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployee;
using MarketStat.Services.Dimensions.DimEmployeeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimemployees")]
[Authorize]
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
    [Authorize(Roles = "Analyst, EtlUser")]
    [ProducesResponseType(typeof(IEnumerable<DimEmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = "Analyst, EtlUser")]
    [ProducesResponseType(typeof(DimEmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    public async Task<ActionResult<DimEmployeeDto>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid EmployeeId." });
        }
        var employee = await _dimEmployeeService.GetEmployeeByIdAsync(id);
        var dto = _mapper.Map<DimEmployeeDto>(employee);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new employee.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimEmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] 
    public async Task<ActionResult<DimEmployeeDto>> PostEmployee([FromBody] CreateDimEmployeeDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _dimEmployeeService.CreateEmployeeAsync(
            createDto.EmployeeRefId,
            createDto.BirthDate,
            createDto.CareerStartDate,
            createDto.Gender
        );
        var dto = _mapper.Map<DimEmployeeDto>(created);

        return CreatedAtAction(nameof(GetById), new { id = dto.EmployeeId }, dto);
    }
    
    /// <summary>
    /// Updates an existing employee.
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
    public async Task<IActionResult> PutEmployee(int id, [FromBody] UpdateDimEmployeeDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0) 
        {
            return BadRequest(new { Message = "Invalid EmployeeId." });
        }

        await _dimEmployeeService.UpdateEmployeeAsync(
            id,
            updateDto.EmployeeRefId,
            updateDto.BirthDate,
            updateDto.CareerStartDate,
            updateDto.Gender
        );

        return NoContent();
    }
    
    /// <summary>
    /// Deletes an employee
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
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid EmployeeId." });
        }
        await _dimEmployeeService.DeleteEmployeeAsync(id);
        return NoContent();
    }
}
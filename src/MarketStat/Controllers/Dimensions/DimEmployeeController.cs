namespace MarketStat.Controllers.Dimensions;

using AutoMapper;
using MarketStat.Common.Dto.Dimensions.DimEmployee;
using MarketStat.Services.Dimensions.DimEmployeeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<DimEmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimEmployeeDto>>> GetAll()
    {
        var list = await _dimEmployeeService.GetAllEmployeesAsync().ConfigureAwait(false);
        var dtos = _mapper.Map<IEnumerable<DimEmployeeDto>>(list);
        return Ok(dtos);
    }

    /// <summary>
    /// Returns a single employee by ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
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

        var employee = await _dimEmployeeService.GetEmployeeByIdAsync(id).ConfigureAwait(false);
        var dto = _mapper.Map<DimEmployeeDto>(employee);
        return Ok(dto);
    }

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    /// <param name="createDto"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DimEmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DimEmployeeDto>> PostEmployee([FromBody] CreateDimEmployeeDto createDto)
    {
        ArgumentNullException.ThrowIfNull(createDto);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _dimEmployeeService.CreateEmployeeAsync(
            createDto.EmployeeRefId,
            createDto.BirthDate,
            createDto.CareerStartDate,
            createDto.Gender,
            createDto.EducationId,
            createDto.GraduationYear).ConfigureAwait(false);
        var dto = _mapper.Map<DimEmployeeDto>(created);

        return CreatedAtAction(nameof(GetById), new { id = dto.EmployeeId }, dto);
    }

    /// <summary>
    /// Updates an existing employee.
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
    public async Task<IActionResult> PutEmployee(int id, [FromBody] UpdateDimEmployeeDto updateDto)
    {
        ArgumentNullException.ThrowIfNull(updateDto);
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
            updateDto.Gender,
            updateDto.EducationId,
            updateDto.GraduationYear).ConfigureAwait(false);

        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PatchEmployee(int id, [FromBody] PartialUpdateDimEmployeeDto patchDto)
    {
        ArgumentNullException.ThrowIfNull(patchDto);
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid EmployeeId." });
        }

        await _dimEmployeeService.PartialUpdateEmployeeAsync(
            id,
            patchDto.EmployeeRefId,
            patchDto.CareerStartDate,
            patchDto.EducationId,
            patchDto.GraduationYear).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Deletes an employee.
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
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid EmployeeId." });
        }

        await _dimEmployeeService.DeleteEmployeeAsync(id).ConfigureAwait(false);
        return NoContent();
    }
}

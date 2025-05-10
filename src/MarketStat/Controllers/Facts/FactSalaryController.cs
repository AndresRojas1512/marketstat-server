using System.Collections;
using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Facts;

[ApiController]
[Route("api/factsalaries")]
public class FactSalaryController : ControllerBase
{
    private readonly IFactSalaryService _factSalaryService;
    private readonly IMapper _mapper;

    public FactSalaryController(IFactSalaryService factSalaryService, IMapper mapper)
    {
        _factSalaryService = factSalaryService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Returns all salaries.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FactSalaryDto>>> GetAll()
    {
        var list = await _factSalaryService.GetAllFactSalariesAsync();
        var dtos = _mapper.Map<IEnumerable<FactSalaryDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a salary by ID.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FactSalaryDto>> GetById(int id)
    {
        try
        {
            var fact = await _factSalaryService.GetFactSalaryByIdAsync(id);
            var dto = _mapper.Map<FactSalaryDto>(fact);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
    
    /// <summary>
    /// Returns salaries by filter.
    /// </summary>
    /// <param name="filter"></param>
    [HttpGet("query")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FactSalaryDto>>> GetByFilter([FromQuery] FactSalaryFilter filter)
    {
        var list = await _factSalaryService.GetFactSalariesByFilterAsync(filter);
        var dtos = _mapper.Map<IEnumerable<FactSalaryDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Get salary average by filter.
    /// </summary>
    /// <param name="filter"></param>
    [HttpGet("average")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<decimal>> GetAverage([FromQuery] FactSalaryFilter filter)
    {
        var avg = await _factSalaryService.GetAverageSalaryAsync(filter);
        return Ok(avg);
    }
    
    /// <summary>
    /// Creates a new salary record.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FactSalaryDto>> PostFactSalary([FromBody] CreateFactSalaryDto createDto)
    {
        var created = await _factSalaryService.CreateFactSalaryAsync(
            createDto.DateId,
            createDto.CityId,
            createDto.EmployerId,
            createDto.JobRoleId,
            createDto.EmployeeId,
            createDto.SalaryAmount,
            createDto.BonusAmount
        );
        var dto = _mapper.Map<FactSalaryDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.SalaryFactId }, dto);
    }
    
    /// <summary>
    /// Updates an existing salary record.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutFactSalary(int id, [FromBody] UpdateFactSalaryDto updateDto)
    {
        await _factSalaryService.UpdateFactSalaryAsync(
            id,
            updateDto.DateId,
            updateDto.CityId,
            updateDto.EmployerId,
            updateDto.JobRoleId,
            updateDto.EmployeeId,
            updateDto.SalaryAmount,
            updateDto.BonusAmount
        );
        return NoContent();
    }
    
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFactSalary(int id)
    {
        await _factSalaryService.DeleteFactSalaryAsync(id);
        return NoContent();
    }
}
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FactSalaryDto>>> GetAll()
    {
        var list = await _factSalaryService.GetAllFactSalariesAsync();
        var dtos = _mapper.Map<IEnumerable<FactSalaryDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
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

    [HttpGet("query")]
    public async Task<ActionResult<IEnumerable<FactSalaryDto>>> Query([FromQuery] FactSalaryFilter filter)
    {
        var list = await _factSalaryService.GetFactSalariesByFilterAsync(filter);
        var dtos = _mapper.Map<IEnumerable<FactSalaryDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("average")]
    public async Task<ActionResult<decimal>> GetAverage([FromQuery] FactSalaryFilter filter)
    {
        var avg = await _factSalaryService.GetAverageSalaryAsync(filter);
        return Ok(avg);
    }

    [HttpPost]
    public async Task<ActionResult<FactSalaryDto>> PostFactSalary([FromBody] CreateFactSalaryDto createDto)
    {
        try
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
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutFactSalary(int id, [FromBody] UpdateFactSalaryDto updateDto)
    {
        try
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
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    public async Task<IActionResult> DeleteFactSalary(int id)
    {
        try
        {
            await _factSalaryService.DeleteFactSalaryAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/[controller]")]
public class DimEmployerController : ControllerBase
{
    private readonly IDimEmployerService _dimEmployerService;
    private readonly IMapper _mapper;

    public DimEmployerController(IDimEmployerService dimEmployerService, IMapper mapper)
    {
        _dimEmployerService = dimEmployerService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimEmployerDto>>> GetAll()
    {
        var list = await _dimEmployerService.GetAllEmployersAsync();
        var dtos = _mapper.Map<IEnumerable<DimEmployerDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimEmployerDto>> GetById(int id)
    {
        try
        {
            var employer = await _dimEmployerService.GetEmployerByIdAsync(id);
            var dto = _mapper.Map<DimEmployerDto>(employer);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimEmployerDto>> PostEmployer([FromBody] CreateDimEmployerDto createDto)
    {
        try
        {
            var created = await _dimEmployerService.CreateEmployerAsync(createDto.EmployerName, createDto.IsPublic);
            var dto = _mapper.Map<DimEmployerDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = dto.EmployerId }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutEmployer(int id, [FromBody] UpdateDimEmployerDto updateDto)
    {
        try
        {
            await _dimEmployerService.UpdateEmployerAsync(id, updateDto.EmployerName, updateDto.IsPublic);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployer(int id)
    {
        try
        {
            await _dimEmployerService.DeleteEmployerAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}
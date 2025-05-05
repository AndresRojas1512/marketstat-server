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
    public async Task<ActionResult<IEnumerable<DimEmployerDto>>> GetAllEmployers()
    {
        var employers = await _dimEmployerService.GetAllEmployersAsync();
        var employerDtos = _mapper.Map<IEnumerable<DimEmployerDto>>(employers);
        return Ok(employerDtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimEmployerDto>> GetEmployerById(int employerId)
    {
        try
        {
            var employer = await _dimEmployerService.GetEmployerByIdAsync(employerId);
            return Ok(_mapper.Map<DimEmployerDto>(employer));
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimEmployerDto>> CreateEmployer([FromBody] CreateDimEmployerDto inputEmployer)
    {
        var createdEmployer = await _dimEmployerService.CreateEmployerAsync(inputEmployer.EmployerName, inputEmployer.IsPublic);
        var employerDto = _mapper.Map<DimEmployerDto>(createdEmployer);
        return CreatedAtAction(nameof(GetEmployerById), new { id = employerDto.EmployerId }, employerDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DimEmployerDto>> UpdateEmployer(int employerId,
        [FromBody] CreateDimEmployerDto inputEmployer)
    {
        try
        {
            var updatedEmployer =
                await _dimEmployerService.UpdateEmployerAsync(employerId, inputEmployer.EmployerName,
                    inputEmployer.IsPublic);
            return Ok(_mapper.Map<DimEmployerDto>(updatedEmployer));
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployer(int employerId)
    {
        try
        {
            await _dimEmployerService.DeleteEmployerAsync(employerId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}
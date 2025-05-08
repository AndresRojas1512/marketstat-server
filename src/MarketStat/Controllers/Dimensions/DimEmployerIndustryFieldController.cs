using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployerIndustryField;
using MarketStat.Services.Dimensions.DimEmployerIndustryFieldService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimemployerindustryfields")]
public class DimEmployerIndustryFieldController : ControllerBase
{
    private readonly IDimEmployerIndustryFieldService _dimEmployerIndustryFieldService;
    private readonly IMapper _mapper;

    public DimEmployerIndustryFieldController(IDimEmployerIndustryFieldService dimEmployerIndustryFieldService,
        IMapper mapper)
    {
        _dimEmployerIndustryFieldService = dimEmployerIndustryFieldService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimEmployerIndustryFieldDto>>> GetAll()
    {
        var list = await _dimEmployerIndustryFieldService.GetAllEmployerIndustryFieldsAsync();
        var dtos = _mapper.Map<IEnumerable<DimEmployerIndustryFieldDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{employerId:int}/{industryFieldId:int}")]
    public async Task<ActionResult<DimEmployerIndustryFieldDto>> GetByEmployerIdIndustryFieldId(int employerId,
        int industryFieldId)
    {
        try
        {
            var link = await _dimEmployerIndustryFieldService.GetEmployerIndustryFieldAsync(employerId, industryFieldId);
            var dto = _mapper.Map<DimEmployerIndustryFieldDto>(link);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpGet("byemployer/{employerId:int}")]
    public async Task<ActionResult<IEnumerable<DimEmployerIndustryFieldDto>>> GetIndustryFieldsByEmployer(
        int employerId)
    {
        var list = await _dimEmployerIndustryFieldService.GetIndustryFieldsByEmployerIdAsync(employerId);
        var dtos = _mapper.Map<IEnumerable<DimEmployerIndustryFieldDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("byindustryfield/{industryFieldId:int}")]
    public async Task<ActionResult<IEnumerable<DimEmployerIndustryFieldDto>>> GetEmployersByIndustryField(
        int industryFieldId)
    {
        var list = await _dimEmployerIndustryFieldService.GetEmployersByIndustryFieldIdAsync(industryFieldId);
        var dtos = _mapper.Map<IEnumerable<DimEmployerIndustryFieldDto>>(list);
        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<DimEmployerIndustryFieldDto>> PostEmployerIndustryField(
        [FromBody] CreateDimEmployerIndustryFieldDto createDto)
    {
        try
        {
            var created =
                await _dimEmployerIndustryFieldService.CreateEmployerIndustryFieldAsync(createDto.EmployerId,
                    createDto.IndustryFieldId);
            var dto = _mapper.Map<DimEmployerIndustryFieldDto>(created);
            return CreatedAtAction(nameof(GetByEmployerIdIndustryFieldId),
                new { employerId = dto.EmployerId, industryFieldId = dto.IndustryFieldId }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{employerId:int}/{industryFieldId:int}")]
    public async Task<IActionResult> DeleteEmployerIndustryField(int employerId, int industryFieldId)
    {
        try
        {
            await _dimEmployerIndustryFieldService.DeleteEmployerIndustryFieldAsync(employerId, industryFieldId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
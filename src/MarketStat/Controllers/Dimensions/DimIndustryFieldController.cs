using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/industryfields")]
public class DimIndustryFieldController : ControllerBase
{
    private readonly IDimIndustryFieldService _dimIndustryFieldService;
    private readonly IMapper _mapper;

    public DimIndustryFieldController(IDimIndustryFieldService dimIndustryFieldService, IMapper mapper)
    {
        _dimIndustryFieldService = dimIndustryFieldService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimIndustryFieldDto>>> GetAll()
    {
        var list = await _dimIndustryFieldService.GetAllIndustryFieldsAsync();
        var dtos = _mapper.Map<IEnumerable<DimIndustryFieldDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimIndustryFieldDto>> GetById(int id)
    {
        try
        {
            var field = await _dimIndustryFieldService.GetIndustryFieldByIdAsync(id);
            var dto = _mapper.Map<DimIndustryFieldDto>(field);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimIndustryFieldDto>> PostIndustryField(
        [FromBody] CreateDimIndustryFieldDto createDto)
    {
        try
        {
            var created = _dimIndustryFieldService.CreateIndustryFieldAsync(createDto.IndustryFieldName);
            var dto = _mapper.Map<DimIndustryFieldDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutIndustryField(int id, [FromBody] UpdateDimIndustryFieldDto updateDto)
    {
        try
        {
            await _dimIndustryFieldService.UpdateIndustryFieldAsync(id, updateDto.IndustryFieldName);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteIndustryField(int id)
    {
        try
        {
            await _dimIndustryFieldService.DeleteIndustryFieldAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
    
}
using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimStandardJobRole;
using MarketStat.Services.Dimensions.DimStandardJobRoleService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimstandardjobroles")]
public class DimStandardJobRoleController : ControllerBase
{
    private readonly IDimStandardJobRoleService _dimStandardJobRoleService;
    private readonly IMapper _mapper;

    public DimStandardJobRoleController(IDimStandardJobRoleService dimStandardJobRoleService, IMapper mapper)
    {
        _dimStandardJobRoleService = dimStandardJobRoleService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimStandardJobRoleDto>>> GetStandardJobRoles()
    {
        var list = await _dimStandardJobRoleService.GetAllStandardJobRolesAsync();
        var dtos = _mapper.Map<IEnumerable<DimStandardJobRoleDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimStandardJobRoleDto>> GetStandardJobRole(int id)
    {
        try
        {
            var jobRole = await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(id);
            var dto = _mapper.Map<DimStandardJobRoleDto>(jobRole);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimStandardJobRoleDto>> PostStandardJobRole(DimStandardJobRoleDto createDto)
    {
        try
        {
            var created =
                await _dimStandardJobRoleService.CreateStandardJobRoleAsync(createDto.StandardJobRoleTitle,
                    createDto.IndustryFieldId);
            var dto = _mapper.Map<DimStandardJobRoleDto>(created);
            return CreatedAtAction(nameof(GetStandardJobRole), new { id = dto.StandardJobRoleId }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutStandardJobRole(int id, UpdateDimStandardJobRoleDto updateDto)
    {
        try
        {
            await _dimStandardJobRoleService.UpdateStandardJobRoleAsync(id, updateDto.StandardJobRoleTitle,
                updateDto.IndustryFieldId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteStandardJobRole(int id)
    {
        try
        {
            await _dimStandardJobRoleService.DeleteStandardJobRoleAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
    
}
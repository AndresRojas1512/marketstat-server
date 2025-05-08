using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJobRole;
using MarketStat.Services.Dimensions.DimJobRoleService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimjobroles")]
public class DimJobRoleController : ControllerBase
{
    private readonly IDimJobRoleService _dimJobRoleService;
    private readonly IMapper _mapper;

    public DimJobRoleController(IDimJobRoleService dimJobRoleService, IMapper mapper)
    {
        _dimJobRoleService = dimJobRoleService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DimJobRoleDto>>> GetAll()
    {
        var list = await _dimJobRoleService.GetAllJobRolesAsync();
        var dtos = _mapper.Map<IEnumerable<DimJobRoleDto>>(list);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DimJobRoleDto>> GetById(int id)
    {
        try
        {
            var job = await _dimJobRoleService.GetJobRoleByIdAsync(id);
            var dto = _mapper.Map<DimJobRoleDto>(job);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DimJobRoleDto>> PostJobRole([FromBody] CreateDimJobRoleDto createDto)
    {
        try
        {
            var created = await _dimJobRoleService.CreateJobRoleAsync(createDto.JobRoleTitle,
                createDto.StandardJobRoleId, createDto.HierarchyLevelId);
            var dto = _mapper.Map<DimJobRoleDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = dto.JobRoleId }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutJobRole(int id, [FromBody] UpdateDimJobRoleDto updateDto)
    {
        try
        {
            await _dimJobRoleService.UpdateJobRoleAsync(id, updateDto.JobRoleTitle, updateDto.StandardJobRoleId,
                updateDto.HierarchyLevelId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteJobRole(int id)
    {
        try
        {
            await _dimJobRoleService.DeleteJobRoleAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
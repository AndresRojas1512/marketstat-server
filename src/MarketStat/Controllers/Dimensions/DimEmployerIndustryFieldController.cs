using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployerIndustryField;
using MarketStat.Services.Dimensions.DimEmployerIndustryFieldService;
using Microsoft.AspNetCore.Authorization;
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
    
    /// <summary>
    /// Returns all Employer-IndustryField links.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimEmployerIndustryFieldDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimEmployerIndustryFieldDto>>> GetAll()
    {
        var list = await _dimEmployerIndustryFieldService.GetAllEmployerIndustryFieldsAsync();
        var dtos = _mapper.Map<IEnumerable<DimEmployerIndustryFieldDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Get a specific Employer-IndustryField link.
    /// </summary>
    /// <param name="employerId"></param>
    /// <param name="industryFieldId"></param>
    [HttpGet("{employerId:int}/{industryFieldId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimEmployerIndustryFieldDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimEmployerIndustryFieldDto>> GetByEmployerIdIndustryFieldId(int employerId,
        int industryFieldId)
    {
        if (employerId <= 0 || industryFieldId <= 0)
        {
            return BadRequest(new { Message = "Invalid EmployerId or IndustryFieldId." });
        }
        var link = await _dimEmployerIndustryFieldService.GetEmployerIndustryFieldAsync(employerId, industryFieldId);
        var dto = _mapper.Map<DimEmployerIndustryFieldDto>(link);
        return Ok(dto);
    }
    
    /// <summary>
    /// Returns all IndustryField links for a given Employer.
    /// </summary>
    /// <param name="employerId"></param>
    /// <returns></returns>
    [HttpGet("byemployer/{employerId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimEmployerIndustryFieldDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimEmployerIndustryFieldDto>>> GetIndustryFieldsByEmployer(
        int employerId)
    {
        if (employerId <= 0)
        {
            return BadRequest(new { Message = "Invalid EmployerId." });
        }
        var list = await _dimEmployerIndustryFieldService.GetIndustryFieldsByEmployerIdAsync(employerId);
        var dtos = _mapper.Map<IEnumerable<DimEmployerIndustryFieldDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns all Employer links for a given IndustryField.
    /// </summary>
    /// <param name="industryFieldId"></param>
    /// <returns></returns>
    [HttpGet("byindustryfield/{industryFieldId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimEmployerIndustryFieldDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimEmployerIndustryFieldDto>>> GetEmployersByIndustryField(
        int industryFieldId)
    {
        if (industryFieldId <= 0)
        {
            return BadRequest(new { Message = "Invalid IndustryFieldId." });
        }
        var list = await _dimEmployerIndustryFieldService.GetEmployersByIndustryFieldIdAsync(industryFieldId);
        var dtos = _mapper.Map<IEnumerable<DimEmployerIndustryFieldDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Creates a new Employer-IndustryField link.
    /// </summary>
    /// <param name="createDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimEmployerIndustryFieldDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] 
    public async Task<ActionResult<DimEmployerIndustryFieldDto>> PostEmployerIndustryField(
        [FromBody] CreateDimEmployerIndustryFieldDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var created =
            await _dimEmployerIndustryFieldService.CreateEmployerIndustryFieldAsync(createDto.EmployerId,
                createDto.IndustryFieldId);
        var dto = _mapper.Map<DimEmployerIndustryFieldDto>(created);
        return CreatedAtAction(nameof(GetByEmployerIdIndustryFieldId),
            new { employerId = dto.EmployerId, industryFieldId = dto.IndustryFieldId }, dto);
    }

    [HttpDelete("{employerId:int}/{industryFieldId:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployerIndustryField(int employerId, int industryFieldId)
    {
        if (employerId <= 0 || industryFieldId <= 0)
        {
            return BadRequest(new { Message = "Invalid EmployerId or IndustryFieldId." });
        }
        await _dimEmployerIndustryFieldService.DeleteEmployerIndustryFieldAsync(employerId, industryFieldId);
        return NoContent();
    }
}
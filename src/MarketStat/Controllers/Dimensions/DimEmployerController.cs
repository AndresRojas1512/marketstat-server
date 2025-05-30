using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimemployers")]
public class DimEmployerController : ControllerBase
{
    private readonly IDimEmployerService _dimEmployerService;
    private readonly IMapper _mapper;

    public DimEmployerController(IDimEmployerService dimEmployerService, IMapper mapper)
    {
        _dimEmployerService = dimEmployerService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Returns all employers.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimEmployerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimEmployerDto>>> GetAll()
    {
        var list = await _dimEmployerService.GetAllEmployersAsync();
        var dtos = _mapper.Map<IEnumerable<DimEmployerDto>>(list);
        return Ok(dtos);
    }
    
    /// <summary>
    /// Returns a single employer by ID.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimEmployerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimEmployerDto>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid EmployerId." });
        }
        var employer = await _dimEmployerService.GetEmployerByIdAsync(id);
        var dto = _mapper.Map<DimEmployerDto>(employer);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new employer.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimEmployerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DimEmployerDto>> PostEmployer([FromBody] CreateDimEmployerDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var created = await _dimEmployerService.CreateEmployerAsync(createDto.EmployerName, createDto.IsPublic);
        var dto = _mapper.Map<DimEmployerDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.EmployerId }, dto);
    }
    
    /// <summary>
    /// Updates an existing employer.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutEmployer(int id, [FromBody] UpdateDimEmployerDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0) 
        {
            return BadRequest(new { Message = "Invalid EmployerId." });
        }
        await _dimEmployerService.UpdateEmployerAsync(id, updateDto.EmployerName, updateDto.IsPublic);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes an employer.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteEmployer(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Invalid EmployerId." });
        }
        await _dimEmployerService.DeleteEmployerAsync(id);
        return NoContent();
    }
}
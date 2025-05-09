using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/employers")]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DimEmployerDto>> GetById(int id)
    {
        var employer = await _dimEmployerService.GetEmployerByIdAsync(id);
        var dto = _mapper.Map<DimEmployerDto>(employer);
        return Ok(dto);
    }
    
    /// <summary>
    /// Creates a new employer.
    /// </summary>
    /// <param name="createDto"></param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimEmployerDto>> PostEmployer([FromBody] CreateDimEmployerDto createDto)
    {
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutEmployer(int id, [FromBody] UpdateDimEmployerDto updateDto)
    {
        await _dimEmployerService.UpdateEmployerAsync(id, updateDto.EmployerName, updateDto.IsPublic);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes an employer.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployer(int id)
    {
        await _dimEmployerService.DeleteEmployerAsync(id);
        return NoContent();
    }
}
using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimOblast;
using MarketStat.Services.Dimensions.DimOblastService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Dimensions;

[ApiController]
[Route("api/dimoblasts")]
public class DimOblastController : ControllerBase
{
    private readonly IDimOblastService _dimOblastService;
    private readonly IMapper _mapper;
    private readonly ILogger<DimOblastController> _logger;

    public DimOblastController(
        IDimOblastService dimOblastService,
        IMapper mapper,
        ILogger<DimOblastController> logger)
    {
        _dimOblastService = dimOblastService ?? throw new ArgumentNullException(nameof(dimOblastService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Returns all oblasts. (Publicly accessible)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimOblastDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimOblastDto>>> GetAll()
    {
        _logger.LogInformation("Attempting to get all oblasts (publicly accessible).");
        var list = await _dimOblastService.GetAllOblastsAsync();
        var dtos = _mapper.Map<IEnumerable<DimOblastDto>>(list);
        _logger.LogInformation("Successfully retrieved {Count} oblast records.", dtos.Count());
        return Ok(dtos);
    }

    /// <summary>
    /// Returns an oblast by ID. (Publicly accessible)
    /// </summary>
    /// <param name="id">The ID of the oblast.</param>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DimOblastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DimOblastDto>> GetById(int id)
    {
        _logger.LogInformation("Attempting to get oblast by ID: {OblastId} (publicly accessible).", id);
        if (id <= 0)
        {
            _logger.LogWarning("GetById called with invalid OblastId: {OblastId}", id);
            return BadRequest(new { Message = "Invalid OblastId." });
        }
        var oblast = await _dimOblastService.GetOblastByIdAsync(id);
        var dto = _mapper.Map<DimOblastDto>(oblast);
        _logger.LogInformation("Successfully retrieved oblast by ID: {OblastId}.", id);
        return Ok(dto);
    }

    /// <summary>
    /// Returns oblasts by federal district ID. (Publicly accessible)
    /// </summary>
    /// <param name="districtId">The ID of the federal district.</param>
    [HttpGet("bydistrict/{districtId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DimOblastDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DimOblastDto>>> GetOblastsByFederalDistrict(int districtId)
    {
        _logger.LogInformation("Attempting to get oblasts by FederalDistrictId: {DistrictId} (publicly accessible).", districtId);
        if (districtId <= 0)
        {
            _logger.LogWarning("GetOblastsByFederalDistrict called with invalid DistrictId: {DistrictId}", districtId);
            return BadRequest(new { Message = "Invalid DistrictId." });
        }
        var list = await _dimOblastService.GetOblastsByFederalDistrictIdAsync(districtId);
        var dtos = _mapper.Map<IEnumerable<DimOblastDto>>(list);
        _logger.LogInformation("Successfully retrieved {Count} oblasts for FederalDistrictId: {DistrictId}.", dtos.Count(), districtId);
        return Ok(dtos);
    }

    /// <summary>
    /// Creates a new oblast. (Requires "EtlUser" role)
    /// </summary>
    /// <param name="createDto">The DTO containing data for the new oblast.</param>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(typeof(DimOblastDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DimOblastDto>> PostOblast([FromBody] CreateDimOblastDto createDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("PostOblast: Invalid model state: {@ModelStateErrors}", ModelState);
            return BadRequest(ModelState);
        }
        _logger.LogInformation("User (EtlUser) attempting to create oblast: {OblastName} in DistrictId: {DistrictId}", 
            createDto.OblastName, createDto.DistrictId);
        
        var created = await _dimOblastService.CreateOblastAsync(createDto.OblastName, createDto.DistrictId);
        var dto = _mapper.Map<DimOblastDto>(created);

        _logger.LogInformation("Oblast '{OblastName}' created successfully with ID {OblastId}.", dto.OblastName, dto.OblastId);
        return CreatedAtAction(nameof(GetById), new { id = dto.OblastId }, dto);
    }

    /// <summary>
    /// Updates an existing oblast. (Requires "EtlUser" role)
    /// </summary>
    /// <param name="id">The ID of the oblast to update.</param>
    /// <param name="updateDto">The DTO containing updated data.</param>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutOblast(int id, [FromBody] UpdateDimOblastDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("PutOblast: Invalid model state for OblastId {OblastId}: {@ModelStateErrors}", id, ModelState);
            return BadRequest(ModelState);
        }
        if (id <= 0) 
        {
            _logger.LogWarning("PutOblast called with invalid OblastId: {OblastId}", id);
            return BadRequest(new { Message = "Invalid OblastId." });
        }
        _logger.LogInformation("User (EtlUser) attempting to update oblast ID: {OblastId}", id);

        await _dimOblastService.UpdateOblastAsync(id, updateDto.OblastName, updateDto.DistrictId);

        _logger.LogInformation("Oblast ID: {OblastId} updated successfully.", id);
        return NoContent();
    }

    /// <summary>
    /// Deletes an oblast. (Requires "EtlUser" role)
    /// </summary>
    /// <param name="id">The ID of the oblast to delete.</param>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "EtlUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] 
    public async Task<IActionResult> DeleteOblast(int id)
    {
        if (id <= 0)
        {
             _logger.LogWarning("DeleteOblast called with invalid OblastId: {OblastId}", id);
             return BadRequest(new { Message = "Invalid OblastId." });
        }
        _logger.LogInformation("User (EtlUser) attempting to delete oblast ID: {OblastId}", id);

        await _dimOblastService.DeleteOblastAsync(id);

        _logger.LogInformation("Oblast ID: {OblastId} deleted successfully.", id);
        return NoContent();
    }
}
using System.Security.Claims;
using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Facts;

[ApiController]
[Route("api/factsalaries")]
[Authorize]
public class FactSalaryController : ControllerBase
{
    private readonly IFactSalaryService _factSalaryService;
    private readonly IMapper _mapper;
    private readonly ILogger<FactSalaryController> _logger;

    public FactSalaryController(IFactSalaryService factSalaryService, IMapper mapper, ILogger<FactSalaryController> logger)
    {
        _factSalaryService = factSalaryService ?? throw new ArgumentNullException(nameof(factSalaryService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a specific salary fact record by its ID.
    /// </summary>
    /// <param name="id">The ID of the salary fact.</param>
    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FactSalaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FactSalaryDto>> GetSalaryById(long id)
    {
        _logger.LogInformation("Attempting to get salary fact with ID: {SalaryFactId}", id);
        if (id <= 0)
        {
            _logger.LogWarning("GetSalaryById called with invalid SalaryFactId: {SalaryFactId}", id);
            return BadRequest(new { Message = "Invalid SalaryFactId." });
        }
        var salaryFactDomain = await _factSalaryService.GetFactSalaryByIdAsync(id);
        var salaryFactDto = _mapper.Map<FactSalaryDto>(salaryFactDomain);
        _logger.LogInformation("Successfully retrieved salary fact with ID: {SalaryFactId}", id);
        return Ok(salaryFactDto);
    }

    /// <summary>
    /// Gets salary fact records based on filter criteria (using IDs).
    /// </summary>
    /// <param name="filterDto">The filter criteria.</param>
    [HttpGet("byfilter")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<FactSalaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<FactSalaryDto>>> GetSalariesByFilter([FromQuery] SalaryFilterDto filterDto)
    {
        _logger.LogInformation("Controller: Attempting to get salary facts by filter: {@FilterDto}", filterDto);
        var salaryFactsDomain = await _factSalaryService.GetFactSalariesByFilterAsync(filterDto);
        var salaryFactDtos = _mapper.Map<IEnumerable<FactSalaryDto>>(salaryFactsDomain);
        _logger.LogInformation("Controller: Successfully retrieved {Count} salary facts for filter.", salaryFactDtos.Count());
        return Ok(salaryFactDtos);
        
    }

    /// <summary>
    /// Creates a new salary fact record.
    /// </summary>
    /// <param name="createDto">The DTO containing data for the new salary fact.</param>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FactSalaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FactSalaryDto>> CreateSalaryFact([FromBody] CreateFactSalaryDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _logger.LogInformation("Attempting to create a new salary fact with data: {@CreateDto}", createDto);
        
        var createdSalaryFactDomain = await _factSalaryService.CreateFactSalaryAsync(
            createDto.DateId, createDto.LocationId, createDto.EmployerId, createDto.JobId,
            createDto.EmployeeId, createDto.SalaryAmount
        );
        var resultDto = _mapper.Map<FactSalaryDto>(createdSalaryFactDomain);
        _logger.LogInformation("Successfully created salary fact with ID: {SalaryFactId}", resultDto.SalaryFactId);
        return CreatedAtAction(nameof(GetSalaryById), new { id = resultDto.SalaryFactId }, resultDto);
    }

    /// <summary>
    /// Updates an existing salary fact record.
    /// </summary>
    /// <param name="id">The ID of the salary fact to update.</param>
    /// <param name="updateDto">The DTO containing updated data.</param>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateSalaryFact(long id, [FromBody] UpdateFactSalaryDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0) 
        {
            return BadRequest(new { Message = "Invalid SalaryFactId." });
        }
        _logger.LogInformation("Attempting to update salary fact with ID: {SalaryFactId} using data: {@UpdateDto}", id, updateDto);
        
        await _factSalaryService.UpdateFactSalaryAsync(
            id, updateDto.DateId, updateDto.LocationId, updateDto.EmployerId, updateDto.JobId,
            updateDto.EmployeeId, updateDto.SalaryAmount
        );
        _logger.LogInformation("Successfully updated salary fact with ID: {SalaryFactId}", id);
        return NoContent();
    }

    /// <summary>
    /// Deletes a salary fact record by its ID.
    /// </summary>
    /// <param name="id">The ID of the salary fact to delete.</param>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSalaryFact(long id)
    {
        if (id <= 0)
        {
             return BadRequest(new { Message = "Invalid SalaryFactId." });
        }
        _logger.LogInformation("Attempting to delete salary fact with ID: {SalaryFactId}", id);
        await _factSalaryService.DeleteFactSalaryAsync(id); 
        _logger.LogInformation("Successfully deleted salary fact with ID: {SalaryFactId}", id);
        return NoContent();
    }
    
    // Authorized analytical endpoints

    [HttpGet("distribution")]
    [Authorize(Roles = "Admin, Analyst")]
    [ProducesResponseType(typeof(List<SalaryDistributionBucketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<SalaryDistributionBucketDto>>> GetSalaryDistribution(
        [FromQuery] SalaryFilterDto filters)
    {
        _logger.LogInformation("User requesting salary distribution: {@Filters}", filters);
        var data = await _factSalaryService.GetSalaryDistributionAsync(filters);
        return Ok(data);
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Admin, Analyst")]
    [ProducesResponseType(typeof(SalarySummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SalarySummaryDto>> GetSalarySummary([FromQuery] SalaryFilterDto filters,
        [FromQuery] int targetPercentile = 90)
    {
        _logger.LogInformation("User requesting salary summary: {@Filters}, Percentile: {Percentile}", filters,
            targetPercentile);
        var data = await _factSalaryService.GetSalarySummaryAsync(filters, targetPercentile);
        return Ok(data);
    }

    [HttpGet("timeseries")]
    [Authorize(Roles = "Admin, Analyst")]
    [ProducesResponseType(typeof(List<SalaryTimeSeriesPointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<SalaryTimeSeriesPointDto>>> GetSalaryTimeSeries(
        [FromQuery] SalaryFilterDto filters, [FromQuery] TimeGranularity granularity = TimeGranularity.Month,
        [FromQuery] int periods = 12)
    {
        _logger.LogInformation(
            "User requesting salary time series: {@Filters}, Granularity: {Granularity}, Periods: {Periods}", filters,
            granularity, periods);
        var data = await _factSalaryService.GetSalaryTimeSeriesAsync(filters, granularity, periods);
        return Ok(data);
    }
    
    // Public analytical endpoints
    
    [HttpGet("public/roles")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PublicRoleByLocationIndustryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PublicRoleByLocationIndustryDto>>> GetPublicRoles(
        [FromQuery] SalaryFilterDto filters, [FromQuery] int minRecordCount = 10)
    {
        _logger.LogInformation("Public request for roles: {@Filters}", filters);
        var data = await _factSalaryService.GetPublicRolesAsync(filters, minRecordCount);
        return Ok(data);
    }
}
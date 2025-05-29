using System.Collections;
using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MarketStat.Controllers.Facts;

[ApiController]
[Route("api/salaries")]
// [Authorize]
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
    /// Gets all salary fact records.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FactSalaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FactSalaryDto>>> GetAllSalaries()
    {
        _logger.LogInformation("Attempting to get all salary facts.");
        var salaryFactsDomain = await _factSalaryService.GetAllFactSalariesAsync();
        var salaryFactDtos = _mapper.Map<IEnumerable<FactSalaryDto>>(salaryFactsDomain);
        _logger.LogInformation("Successfully retrieved {Count} salary facts.", salaryFactDtos.Count());
        return Ok(salaryFactDtos);
    }

    /// <summary>
    /// Gets a specific salary fact record by its ID.
    /// </summary>
    /// <param name="id">The ID of the salary fact.</param>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(FactSalaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FactSalaryDto>> GetSalaryById(long id)
    {
        _logger.LogInformation("Attempting to get salary fact with ID: {SalaryFactId}", id);
        if (id <= 0)
        {
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
    [HttpGet("query")]
    [ProducesResponseType(typeof(IEnumerable<FactSalaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FactSalaryDto>>> GetSalariesByFilter([FromQuery] SalaryFilterDto filterDto)
    {
        _logger.LogInformation("Attempting to get salary facts by filter: {@FilterDto}", filterDto);
        var salaryFactsDomain = await _factSalaryService.GetFactSalariesByFilterAsync(filterDto);
        var salaryFactDtos = _mapper.Map<IEnumerable<FactSalaryDto>>(salaryFactsDomain);
        _logger.LogInformation("Successfully retrieved {Count} salary facts for filter: {@FilterDto}", salaryFactDtos.Count(), filterDto);
        return Ok(salaryFactDtos);
    }

    /// <summary>
    /// Creates a new salary fact record.
    /// </summary>
    /// <param name="createDto">The DTO containing data for the new salary fact.</param>
    [HttpPost]
    [ProducesResponseType(typeof(FactSalaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            createDto.DateId, createDto.CityId, createDto.EmployerId, createDto.JobRoleId,
            createDto.EmployeeId, createDto.SalaryAmount, createDto.BonusAmount
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            id,
            updateDto.DateId, updateDto.CityId, updateDto.EmployerId, updateDto.JobRoleId,
            updateDto.EmployeeId, updateDto.SalaryAmount, updateDto.BonusAmount
        );
        _logger.LogInformation("Successfully updated salary fact with ID: {SalaryFactId}", id);
        return NoContent();
    }

    /// <summary>
    /// Deletes a salary fact record by its ID.
    /// </summary>
    /// <param name="id">The ID of the salary fact to delete.</param>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    
    // --- Analytical Endpoints ---

    /// <summary>
    /// Gets a consolidated salary benchmarking report.
    /// </summary>
    /// <param name="filters">Filter criteria for the report (ID-based).</param>
    [HttpGet("benchmarking-report")]
    [ProducesResponseType(typeof(BenchmarkDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BenchmarkDataDto>> GetBenchmarkingReport([FromQuery] BenchmarkQueryDto filters)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _logger.LogInformation("Received request for benchmarking report: {@Filters}", filters);
        BenchmarkDataDto? reportData = await _factSalaryService.GetBenchmarkingReportAsync(filters);
        return Ok(reportData); 
    }

    /// <summary>
    /// Gets salary distribution (histogram buckets).
    /// </summary>
    /// <param name="filters">ID-based filter criteria.</param>
    [HttpGet("distribution")]
    [ProducesResponseType(typeof(List<SalaryDistributionBucketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<SalaryDistributionBucketDto>>> GetSalaryDistribution([FromQuery] SalaryFilterDto filters)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _logger.LogInformation("Fetching salary distribution for filters: {@Filters}", filters);
        var distribution = await _factSalaryService.GetSalaryDistributionAsync(filters);
        return Ok(distribution); 
    }

    /// <summary>
    /// Gets salary summary statistics (percentiles, average, count).
    /// </summary>
    /// <param name="filters">ID-based filter criteria.</param>
    /// <param name="targetPercentile">The target percentile to calculate.</param>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(SalarySummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    public async Task<ActionResult<SalarySummaryDto>> GetSalarySummary([FromQuery] SalaryFilterDto filters, [FromQuery] int targetPercentile = 90)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _logger.LogInformation("Fetching salary summary for filters: {@Filters}, target percentile: {TargetPercentile}", filters, targetPercentile);
        var summary = await _factSalaryService.GetSalarySummaryAsync(filters, targetPercentile);
        if (summary == null) // Service returns null if no data for summary
        {
            _logger.LogInformation("No salary summary data found for filters: {@Filters}, target percentile: {TargetPercentile}", filters, targetPercentile);
            return NotFound(new { Message = "No salary data found matching the specified criteria for summary." });
        }
        return Ok(summary);
    }

    /// <summary>
    /// Gets salary time series data.
    /// </summary>
    /// <param name="filters">ID-based filter criteria.</param>
    /// <param name="granularity">Time granularity (Month, Quarter, Year).</param>
    /// <param name="periods">Number of periods to show.</param>
    [HttpGet("timeseries")]
    [ProducesResponseType(typeof(List<SalaryTimeSeriesPointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<SalaryTimeSeriesPointDto>>> GetSalaryTimeSeries(
        [FromQuery] SalaryFilterDto filters,
        [FromQuery] TimeGranularity granularity = TimeGranularity.Month,
        [FromQuery] int periods = 12)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        _logger.LogInformation("Fetching salary time series for filters: {@Filters}, granularity: {Granularity}, periods: {Periods}", filters, granularity, periods);
        var timeSeries = await _factSalaryService.GetSalaryTimeSeriesAsync(filters, granularity, periods);
        return Ok(timeSeries);
    }

    /// <summary>
    /// Gets a public view of standard job roles by location and industry, ordered by average salary.
    /// </summary>
    /// <param name="industryFieldId">Mandatory: The ID of the industry field.</param>
    /// <param name="federalDistrictId">Optional: Filter by federal district ID.</param>
    /// <param name="oblastId">Optional: Filter by oblast ID.</param>
    /// <param name="cityId">Optional: Filter by city ID.</param>
    /// <param name="minSalaryRecordsForRole">Optional: Minimum records to include a role (default 3).</param>
    [HttpGet("public/roles-by-location-industry")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PublicRoleByLocationIndustryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PublicRoleByLocationIndustryDto>>> GetPublicRolesByLocationIndustry(
        [FromQuery, BindRequired] int industryFieldId,
        [FromQuery] int? federalDistrictId,
        [FromQuery] int? oblastId,
        [FromQuery] int? cityId,
        [FromQuery] int minSalaryRecordsForRole = 3)
    {
        _logger.LogInformation("Public request for roles by location/industry: IndustryId={IndustryId}", industryFieldId);
        // Service method will do more detailed validation if needed
        var result = await _factSalaryService.GetPublicRolesByLocationIndustryAsync(industryFieldId, federalDistrictId, oblastId, cityId, minSalaryRecordsForRole);
        return Ok(result);
    }

    /// <summary>
    /// Gets a public view of top N education degrees for employees in jobs related to a specific industry.
    /// </summary>
    /// <param name="industryFieldId">Mandatory: The ID of the industry field.</param>
    /// <param name="topNDegrees">Optional: Number of top degrees to return (default 5).</param>
    /// <param name="minEmployeeCountForDegree">Optional: Minimum employees with a degree to include it (default 3).</param>
    [HttpGet("public/top-degrees-by-industry")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PublicDegreeByIndustryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PublicDegreeByIndustryDto>>> GetPublicTopDegreesByIndustry(
        [FromQuery, BindRequired] int industryFieldId,
        [FromQuery] int topNDegrees = 5,
        [FromQuery] int minEmployeeCountForDegree = 3)
    {
        _logger.LogInformation("Public request for top degrees by industry: IndustryId={IndustryId}", industryFieldId);
        var result = await _factSalaryService.GetPublicTopDegreesByIndustryAsync(industryFieldId, topNDegrees, minEmployeeCountForDegree);
        return Ok(result);
    }
}
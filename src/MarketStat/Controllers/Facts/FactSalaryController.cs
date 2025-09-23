using System.Collections;
using System.Security.Claims;
using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Etl;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
    /// Gets all salary fact records.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Analyst, EtlUser")]
    [ProducesResponseType(typeof(IEnumerable<FactSalaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = "Analyst, EtlUser")]
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
    [HttpGet("byfilter")] // The method you are updating
    [Authorize(Roles = "Analyst, EtlUser")]
    [ProducesResponseType(typeof(IEnumerable<FactSalaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FactSalaryDto>>> GetSalariesByFilter([FromQuery] SalaryFilterDto filterDto)
    {
        _logger.LogInformation("Controller: Attempting to get salary facts by filter: {@FilterDto}", filterDto);
        try
        {
            var salaryFactsDomain = await _factSalaryService.GetFactSalariesByFilterAsync(filterDto);
            var salaryFactDtos = _mapper.Map<IEnumerable<FactSalaryDto>>(salaryFactsDomain);
            _logger.LogInformation("Controller: Successfully retrieved {Count} salary facts for filter.", salaryFactDtos.Count());
            return Ok(salaryFactDtos);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Controller: Invalid filter arguments provided.");
            return BadRequest(new { Message = "One or more filter IDs are invalid.", Detail = argEx.Message });
        }
    }

    /// <summary>
    /// Creates a new salary fact record.
    /// </summary>
    /// <param name="createDto">The DTO containing data for the new salary fact.</param>
    [HttpPost]
    [Authorize(Roles = "EtlUser")]
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
    [Authorize(Roles = "EtlUser")]
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
    [Authorize(Roles = "EtlUser")]
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
    
    // ===============================
    // Authorized analytical endpoints
    // ===============================

    /// <summary>
    /// Gets a consolidated salary benchmarking report.
    /// </summary>
    /// <param name="filters">Filter criteria for the report (ID-based).</param>
    [HttpGet("benchmarking-report")]
    [Authorize(Roles = "Analyst, EtlUser")] 
    [ProducesResponseType(typeof(BenchmarkDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BenchmarkDataDto>> GetBenchmarkingReport([FromQuery] BenchmarkQueryDto filters)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("User ({UserRoles}) requesting benchmarking report: {@Filters}", 
            string.Join(",", User.FindAll(ClaimTypes.Role).Select(c => c.Value)), filters);
            
        try
        {
            BenchmarkDataDto? reportData = await _factSalaryService.GetBenchmarkingReportAsync(filters);
            return Ok(reportData); 
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Controller: Invalid arguments provided for GetBenchmarkingReport.");
            return BadRequest(new { Message = "One or more filter parameters are invalid.", Detail = argEx.Message });
        }
    }

    /// <summary>
    /// Gets salary distribution (histogram buckets).
    /// </summary>
    /// <param name="filters">ID-based filter criteria.</param>
    [HttpGet("distribution")]
    [Authorize(Roles = "Analyst, EtlUser")]
    [ProducesResponseType(typeof(List<SalaryDistributionBucketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<SalaryDistributionBucketDto>>> GetSalaryDistribution([FromQuery] SalaryFilterDto filters)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _logger.LogInformation("Controller: User ({UserRoles}) fetching salary distribution for filters: {@Filters}", 
            string.Join(",", User.FindAll(ClaimTypes.Role).Select(c => c.Value)), filters);
            
        try
        {
            var distribution = await _factSalaryService.GetSalaryDistributionAsync(filters);
            return Ok(distribution); 
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Controller: Invalid filter arguments provided for GetSalaryDistribution.");
            return BadRequest(new { Message = "One or more filter IDs are invalid.", Detail = argEx.Message });
        }
    }

    /// <summary>
    /// Gets salary summary statistics (percentiles, average, count).
    /// </summary>
    /// <param name="filters">ID-based filter criteria.</param>
    /// <param name="targetPercentile">The target percentile to calculate.</param>
    [HttpGet("summary")]
    [Authorize(Roles = "Analyst, EtlUser")]
    [ProducesResponseType(typeof(SalarySummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    public async Task<ActionResult<SalarySummaryDto>> GetSalarySummary([FromQuery] SalaryFilterDto filters, [FromQuery] int targetPercentile = 90)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
            
        _logger.LogInformation("User ({UserRoles}) fetching salary summary for filters: {@Filters}, target percentile: {TargetPercentile}", 
            string.Join(",", User.FindAll(ClaimTypes.Role).Select(c => c.Value)), filters, targetPercentile);
            
        try
        {
            var summary = await _factSalaryService.GetSalarySummaryAsync(filters, targetPercentile);
            if (summary == null)
            {
                _logger.LogWarning("No salary summary data found for filters {@Filters}", filters);
                return NotFound(new { Message = "No salary data found matching the specified criteria for summary." });
            }
            return Ok(summary);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Controller: Invalid arguments provided for GetSalarySummary.");
            return BadRequest(new { Message = "One or more filter parameters are invalid.", Detail = argEx.Message });
        }
    }

    /// <summary>
    /// Gets salary time series data.
    /// </summary>
    /// <param name="filters">ID-based filter criteria.</param>
    /// <param name="granularity">Time granularity (Month, Quarter, Year).</param>
    /// <param name="periods">Number of periods to show.</param>
    [HttpGet("timeseries")]
    [Authorize(Roles = "Analyst, EtlUser")]
    [ProducesResponseType(typeof(List<SalaryTimeSeriesPointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<SalaryTimeSeriesPointDto>>> GetSalaryTimeSeries(
        [FromQuery] SalaryFilterDto filters,
        [FromQuery] TimeGranularity granularity = TimeGranularity.Month,
        [FromQuery] int periods = 12)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
            
        _logger.LogInformation("User ({UserRoles}) fetching salary time series for filters: {@Filters}, granularity: {Granularity}, periods: {Periods}", 
            string.Join(",", User.FindAll(ClaimTypes.Role).Select(c => c.Value)), filters, granularity, periods);
            
        try
        {
            var timeSeries = await _factSalaryService.GetSalaryTimeSeriesAsync(filters, granularity, periods);
            return Ok(timeSeries);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Controller: Invalid filter arguments provided for GetSalaryTimeSeries.");
            return BadRequest(new { Message = "One or more filter IDs are invalid.", Detail = argEx.Message });
        }
    }
    
    // ==========================
    // Public analytical endpoints
    // ==========================

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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PublicRoleByLocationIndustryDto>>> GetPublicRolesByLocationIndustry(
        [FromQuery] PublicRolesQueryDto queryDto)
    {
        _logger.LogInformation("Public request for roles by location/industry: {@QueryDto}", queryDto);
                
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var result = await _factSalaryService.GetPublicRolesByLocationIndustryAsync(queryDto);
            return Ok(result);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Invalid arguments for GetPublicRolesByLocationIndustryAsync: {@QueryDto}", queryDto);
            return BadRequest(new ProblemDetails { Title = "Invalid query parameters.", Detail = argEx.Message, Status = StatusCodes.Status400BadRequest });
        }
    }
    
    /// <summary>
    /// Gets a public view of average salaries by education specialty and level within a given industry.
    /// </summary>
    /// <param name="queryDto">Query parameters including industry and optional thresholds.</param>
    [HttpGet("public/salary-by-education-in-industry")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PublicSalaryByEducationInIndustryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PublicSalaryByEducationInIndustryDto>>> GetPublicSalaryByEducationInIndustry(
        [FromQuery] PublicSalaryByEducationQueryDto queryDto)
    {
        _logger.LogInformation("Public request for salary by education in industry: {@QueryDto}", queryDto);
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("GetPublicSalaryByEducationInIndustry: Invalid model state: {@ModelStateErrors}", ModelState);
            return BadRequest(ModelState);
        }
    
        try
        {
            var result = await _factSalaryService.GetPublicSalaryByEducationInIndustryAsync(queryDto);
            return Ok(result);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Invalid arguments for GetPublicSalaryByEducationInIndustryAsync: {@QueryDto}", queryDto);
            return BadRequest(new ProblemDetails { Title = "Invalid query parameters.", Detail = argEx.Message, Status = StatusCodes.Status400BadRequest });
        }
    }
    
    /// <summary>
    /// Gets a public view of top employers and their common roles with average salaries within a given industry.
    /// </summary>
    /// <param name="queryDto">Query parameters including industry and optional thresholds.</param>
    [HttpGet("public/top-employer-role-salaries")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PublicTopEmployerRoleSalariesInIndustryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PublicTopEmployerRoleSalariesInIndustryDto>>> GetPublicTopEmployerRoleSalariesInIndustry(
        [FromQuery] PublicTopEmployerRoleSalariesQueryDto queryDto)
    {
        _logger.LogInformation("Public request for top employer role salaries in industry: {@QueryDto}", queryDto);
            
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("GetPublicTopEmployerRoleSalariesInIndustry: Invalid model state: {@ModelStateErrors}", ModelState);
            return BadRequest(ModelState);
        }
    
        try
        {
            var result = await _factSalaryService.GetPublicTopEmployerRoleSalariesInIndustryAsync(queryDto);
            return Ok(result);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Invalid arguments for GetPublicTopEmployerRoleSalariesInIndustryAsync: {@QueryDto}", queryDto);
            return BadRequest(new ProblemDetails { Title = "Invalid query parameters.", Detail = argEx.Message, Status = StatusCodes.Status400BadRequest });
        }
    }
}
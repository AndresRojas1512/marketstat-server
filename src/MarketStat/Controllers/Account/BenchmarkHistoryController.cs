using System.Security.Claims;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Services.Account.BenchmarkHistoryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MarketStat.Controllers.Account;

[ApiController]
[Route("api/benchmarkhistory")]
[Authorize] // All actions in this controller require authentication
public class BenchmarkHistoryController : ControllerBase
{
    private readonly IBenchmarkHistoryService _benchmarkHistoryService;
    private readonly ILogger<BenchmarkHistoryController> _logger;
    // IMapper is not used in this controller as the service returns DTOs directly.

    public BenchmarkHistoryController(
        IBenchmarkHistoryService benchmarkHistoryService,
        ILogger<BenchmarkHistoryController> logger)
    {
        _benchmarkHistoryService = benchmarkHistoryService ?? throw new ArgumentNullException(nameof(benchmarkHistoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private int GetCurrentUserId()
    {
        _logger.LogInformation("--- Attempting to retrieve User ID from claims. All Claims for Current User Principal ---");
        var allClaims = User.Claims.ToList(); // Materialize for easier iteration
        foreach (var claim in allClaims)
        {
            _logger.LogInformation("Claim Type: [{ClaimType}], Value: [{ClaimValue}]", claim.Type, claim.Value);
        }
        _logger.LogInformation("--- End of Claims ---");

        // Find all claims of type NameIdentifier
        var nameIdClaims = allClaims.Where(c => c.Type == ClaimTypes.NameIdentifier).ToList();

        string? userIdString = null;
        int userId = 0;

        if (nameIdClaims.Count == 1)
        {
            // If only one, use it
            userIdString = nameIdClaims[0].Value;
            _logger.LogInformation("Found single ClaimTypes.NameIdentifier: {UserIdString}", userIdString);
        }
        else if (nameIdClaims.Count > 1)
        {
            _logger.LogInformation("Multiple ClaimTypes.NameIdentifier claims found. Attempting to find the numeric one.");
            // Prefer the one that is a parsable integer
            userIdString = nameIdClaims.FirstOrDefault(c => int.TryParse(c.Value, out int _))?.Value;
            if (!string.IsNullOrEmpty(userIdString))
            {
                 _logger.LogInformation("Found numeric ClaimTypes.NameIdentifier among multiple: {UserIdString}", userIdString);
            }
            else
            {
                // Fallback if none of the NameIdentifier claims are numeric (should not happen with your setup)
                _logger.LogInformation("None of the multiple ClaimTypes.NameIdentifier claims were numeric. This is unexpected.");
                // As a last resort, if the direct "nameid" claim from JWT is different and numeric
                var directNameIdClaim = allClaims.FirstOrDefault(c => c.Type == "nameid" && int.TryParse(c.Value, out int _));
                if(directNameIdClaim != null) {
                    userIdString = directNameIdClaim.Value;
                    _logger.LogInformation("Using direct 'nameid' claim as fallback: {UserIdString}", userIdString);
                }
            }
        } else {
             _logger.LogInformation("No ClaimTypes.NameIdentifier claim found. Checking direct 'nameid'.");
             // Fallback: Check for "nameid" directly if ClaimTypes.NameIdentifier isn't found at all
             var directNameIdClaim = allClaims.FirstOrDefault(c => c.Type == "nameid" && int.TryParse(c.Value, out int _));
             if(directNameIdClaim != null) {
                userIdString = directNameIdClaim.Value;
                _logger.LogInformation("Using direct 'nameid' claim: {UserIdString}", userIdString);
             }
        }
        
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out userId) || userId <= 0)
        {
            _logger.LogInformation("User ID claim could not be resolved to a valid positive integer. Final userIdString attempt: '{UserIdString}'", userIdString);
            throw new UnauthorizedAccessException("User ID could not be determined or is invalid from the token.");
        }

        _logger.LogInformation("Successfully resolved current UserId: {UserId} from claim string: '{UserIdString}'", userId, userIdString);
        return userId;
    }

    /// <summary>
    /// Saves a new benchmark analysis for the authenticated user.
    /// </summary>
    /// <param name="saveRequestDto">The benchmark details and filters (using IDs) to save.</param>
    /// <returns>The ID of the newly saved benchmark history record.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)] // Returns an object like { benchmarkHistoryId: newId }
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]   
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SaveBenchmark([FromBody] SaveBenchmarkRequestDto saveRequestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int currentUserId = GetCurrentUserId(); // Can throw UnauthorizedAccessException
        _logger.LogInformation("User {UserId} attempting to save benchmark with name: {BenchmarkName}", currentUserId, saveRequestDto.BenchmarkName);

        long newBenchmarkHistoryId = await _benchmarkHistoryService.SaveCurrentUserBenchmarkAsync(saveRequestDto, currentUserId);

        _logger.LogInformation("Benchmark saved successfully with ID {BenchmarkHistoryId} for User {UserId}", newBenchmarkHistoryId, currentUserId);
        
        return CreatedAtAction(nameof(GetBenchmarkHistoryById), new { id = newBenchmarkHistoryId }, new { benchmarkHistoryId = newBenchmarkHistoryId });
    }

    /// <summary>
    /// Retrieves all saved benchmarks for the authenticated user.
    /// </summary>
    /// <returns>A list of the user's saved benchmarks with resolved filter names.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BenchmarkHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)] 
    public async Task<ActionResult<IEnumerable<BenchmarkHistoryDto>>> GetCurrentUserBenchmarks()
    {
        int currentUserId = GetCurrentUserId(); // Can throw UnauthorizedAccessException
        _logger.LogInformation("Fetching benchmark history for User {UserId}", currentUserId);

        var benchmarks = await _benchmarkHistoryService.GetCurrentUserBenchmarksAsync(currentUserId);
        
        _logger.LogInformation("Retrieved {Count} benchmark history records for User {UserId}", benchmarks.Count(), currentUserId);
        return Ok(benchmarks);
    }

    /// <summary>
    /// Retrieves a specific saved benchmark by its ID for the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the benchmark history record.</param>
    /// <returns>The requested benchmark history details with resolved filter names.</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(BenchmarkHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)] 
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    public async Task<ActionResult<BenchmarkHistoryDto>> GetBenchmarkHistoryById(long id)
    {
        int currentUserId = GetCurrentUserId(); // Can throw UnauthorizedAccessException
        _logger.LogInformation("User {UserId} attempting to fetch benchmark history ID {BenchmarkHistoryId}", currentUserId, id);

        // Service method GetBenchmarkDetailsAsync throws NotFoundException if not found or not owned by user.
        var benchmarkDetails = await _benchmarkHistoryService.GetBenchmarkDetailsAsync(id, currentUserId);

        _logger.LogInformation("Successfully retrieved benchmark history ID {BenchmarkHistoryId} for User {UserId}", id, currentUserId);
        return Ok(benchmarkDetails);
    }

    /// <summary>
    /// Deletes a specific saved benchmark for the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the benchmark history record to delete.</param>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)] 
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    public async Task<IActionResult> DeleteBenchmarkHistory(long id)
    {
        int currentUserId = GetCurrentUserId(); // Can throw UnauthorizedAccessException
        _logger.LogInformation("User {UserId} attempting to delete benchmark history ID {BenchmarkHistoryId}", currentUserId, id);

        // Service method DeleteCurrentUserBenchmarkAsync throws NotFoundException if not found or not owned by user.
        await _benchmarkHistoryService.DeleteCurrentUserBenchmarkAsync(id, currentUserId);

        _logger.LogInformation("Successfully deleted benchmark history ID {BenchmarkHistoryId} for User {UserId}", id, currentUserId);
        return NoContent();
    }
}
using System.Security.Claims;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Services.Account.BenchmarkHistoryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MarketStat.Controllers.Account;

[ApiController]
[Route("api/benchmarkhistory")]
[Authorize]
public class BenchmarkHistoryController : ControllerBase
{
    private readonly IBenchmarkHistoryService _benchmarkHistoryService;
    private readonly ILogger<BenchmarkHistoryController> _logger;

    public BenchmarkHistoryController(
        IBenchmarkHistoryService benchmarkHistoryService,
        ILogger<BenchmarkHistoryController> logger)
    {
        _benchmarkHistoryService = benchmarkHistoryService ?? throw new ArgumentNullException(nameof(benchmarkHistoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("nameid");
        _logger.LogDebug(
            "Attempting to resolve User ID from claims. Found NameIdentifier/nameid value: '{UserIdClaimValue}'",
            userIdClaim);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId <= 0)
        {
            var allClaims = User.Claims.Select(c => $"Type=[{c.Type}], Value=[{c.Value}]").ToList();
            _logger.LogError(
                "User ID claim could not be resolved to a valid positive integer from token. Claims present: {AllClaims}",
                string.Join("; ", allClaims));
            throw new UnauthorizedAccessException("User ID claim not be determined or is invalid from the token.");
        }
        _logger.LogInformation("Successfully resolved current UserId: {UserId}", userId);
        return userId;
    }

    /// <summary>
    /// Saves a new benchmark analysis for the authenticated user.
    /// </summary>
    /// <param name="saveRequestDto">The benchmark details and filters (using IDs) to save.</param>
    /// <returns>The ID of the newly saved benchmark history record.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> SaveBenchmark([FromBody] SaveBenchmarkRequestDto saveRequestDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("SaveBenchmark called with invalid model state: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        int currentUserId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} attempting to save benchmark with name: {BenchmarkName}", currentUserId,
            saveRequestDto.BenchmarkName);
        try
        {
            long newBenchmarkHistoryId =
                await _benchmarkHistoryService.SaveCurrentUserBenchmarkAsync(saveRequestDto, currentUserId);
            _logger.LogInformation("Benchmark saved successfully with ID {BenchmarkHistoryId} for User {UserId}",
                newBenchmarkHistoryId, currentUserId);
            return CreatedAtAction(nameof(GetBenchmarkHistoryById), new { id = newBenchmarkHistoryId },
                new { benchmarkHistoryId = newBenchmarkHistoryId });
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Invalid data provided when saving benchmark for user {UserId}.", currentUserId);
            return BadRequest(new { Message = "Invalid input data.", Detail = argEx.Message });
        }
    }

    /// <summary>
    /// Retrieves all saved benchmarks for the authenticated user.
    /// </summary>
    /// <returns>A list of the user's saved benchmarks with resolved filter names.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BenchmarkHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<BenchmarkHistoryDto>>> GetCurrentUserBenchmarks()
    {
        int currentUserId = GetCurrentUserId();
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    public async Task<ActionResult<BenchmarkHistoryDto>> GetBenchmarkHistoryById(long id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("GetBenchmarkHistoryById called with invalid ID: {BenchmarkHistoryId}", id);
            return BadRequest(new { Message = "Invalid BenchmarkHistoryId" });
        }
        int currentUserId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} attempting to fetch benchmark history ID {BenchmarkHistoryId}",
            currentUserId, id);

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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    public async Task<IActionResult> DeleteBenchmarkHistory(long id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("DeleteBenchmarkHistory called with invalid ID: {BenchmarkHistoryId}", id);
            return BadRequest(new { Message = "Invalid BenchmarkHistoryId." });
        }
        int currentUserId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} deleting benchmark history ID {BenchmarkHistoryId}", currentUserId, id);

        await _benchmarkHistoryService.DeleteCurrentUserBenchmarkAsync(id, currentUserId);

        _logger.LogInformation("Successfully deleted benchmark history ID {BenchmarkHistoryId} for User {UserId}", id, currentUserId);
        return NoContent();
    }
}
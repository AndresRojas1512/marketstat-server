using System.Security.Claims;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Services.Account.BenchmarkHistoryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Account;

[ApiController]
[Route("api/[controller]")] // Results in /api/benchmarkhistory
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
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            _logger.LogError("User ID claim (NameIdentifier) is missing or invalid in the token.");
            throw new UnauthorizedAccessException("User ID could not be determined from the token.");
        }
        return userId;
    }

    /// <summary>
    /// Saves a new benchmark analysis for the authenticated user.
    /// </summary>
    /// <param name="saveRequestDto">The benchmark details to save.</param>
    /// <returns>The ID of the newly saved benchmark history record.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<long>> SaveBenchmark([FromBody] SaveBenchmarkRequestDto saveRequestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int currentUserId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} attempting to save benchmark: {BenchmarkName}", currentUserId, saveRequestDto.BenchmarkName);
        
        long newBenchmarkHistoryId = await _benchmarkHistoryService.SaveCurrentUserBenchmarkAsync(saveRequestDto, currentUserId);

        _logger.LogInformation("Benchmark saved successfully with ID {BenchmarkHistoryId} for User {UserId}", newBenchmarkHistoryId, currentUserId);
        
        return CreatedAtAction(nameof(GetBenchmarkHistoryById), new { id = newBenchmarkHistoryId }, new { benchmarkHistoryId = newBenchmarkHistoryId });
    }

    /// <summary>
    /// Retrieves all saved benchmarks for the authenticated user.
    /// </summary>
    /// <returns>A list of the user's saved benchmarks.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BenchmarkHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <returns>The requested benchmark history details.</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(BenchmarkHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BenchmarkHistoryDto>> GetBenchmarkHistoryById(long id)
    {
        int currentUserId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} attempting to fetch benchmark history ID {BenchmarkHistoryId}", currentUserId, id);
        
        var benchmarkDetails = await _benchmarkHistoryService.GetBenchmarkDetailsAsync(id, currentUserId);

        if (benchmarkDetails == null)
        {
            _logger.LogWarning("Benchmark history ID {BenchmarkHistoryId} not found or not accessible for User {UserId}", id, currentUserId);
            return NotFound(new { Message = $"Benchmark history with ID {id} not found or you do not have permission to view it." });
        }

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBenchmarkHistory(long id)
    {
        int currentUserId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} attempting to delete benchmark history ID {BenchmarkHistoryId}", currentUserId, id);

        bool deleted = await _benchmarkHistoryService.DeleteCurrentUserBenchmarkAsync(id, currentUserId);

        if (!deleted)
        {
            _logger.LogWarning("Failed to delete benchmark history ID {BenchmarkHistoryId} for User {UserId}. Record not found or not owned by user.", id, currentUserId);
            return NotFound(new { Message = $"Benchmark history with ID {id} not found or you do not have permission to delete it." });
        }

        _logger.LogInformation("Successfully deleted benchmark history ID {BenchmarkHistoryId} for User {UserId}", id, currentUserId);
        return NoContent();
    }
}
using System.Text;
using System.Text.Json;
using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;
using MarketStat.Services.Facts.FactSalaryService;
using MarketStat.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Reports;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin,Analyst")]
public class ReportController : ControllerBase
{
    private readonly IFactSalaryService _factSalaryService;
    private readonly IReportStorageService _storageService;
    private readonly IMapper _mapper;
    private readonly ILogger<ReportController> _logger;

    public ReportController(
        IFactSalaryService factSalaryService,
        IReportStorageService storageService,
        IMapper mapper,
        ILogger<ReportController> logger)
    {
        _factSalaryService = factSalaryService;
        _storageService = storageService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost("salary-summary/export")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> ExportSalarySummary([FromBody] SalarySummaryRequestDto requestDto)
    {
        _logger.LogInformation("Generating salary summary for export...");
        var request = _mapper.Map<SalarySummaryRequest>(requestDto);
        var summary = await _factSalaryService.GetSalarySummaryAsync(request);
        if (summary == null)
        {
            return BadRequest("No data found for the specified filters.");
        }
        var jsonContent = JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        var fileBytes = Encoding.UTF8.GetBytes(jsonContent);
        var fileName = $"salary_summary_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.json";
        var url = await _storageService.UploadReportAsync(fileName, fileBytes, "application/json");
        _logger.LogInformation("Report uploaded successfully: {Url}", url);
        return Ok(new { Url = url });
    }
}
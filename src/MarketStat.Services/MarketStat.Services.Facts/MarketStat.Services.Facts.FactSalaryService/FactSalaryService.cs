using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Database.Core.Repositories.Facts;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Facts.FactSalaryService;

public class FactSalaryService : IFactSalaryService
{
    private readonly IFactSalaryRepository _factSalaryRepository;
    private readonly ILogger<FactSalaryService> _logger;

    public FactSalaryService(IFactSalaryRepository factSalaryRepository, ILogger<FactSalaryService> logger)
    {
        _factSalaryRepository = factSalaryRepository;
        _logger = logger;
    }

    public async Task<decimal> GetAverageSalaryAsync(FactSalaryFilter filter)
    {
        var facts = await _factSalaryRepository.GetSalaryByFilterAsync(filter);
        var amounts = facts.Select(f => f.SalaryAmount).ToList();
        if (!amounts.Any())
        {
            _logger.LogInformation("No salary records match filter {@filter}", filter);
            return 0m;
        }
        var avg = (decimal)amounts.Average();
        _logger.LogInformation("Computed average {Average} for filter {@Filter}", avg, filter);
        return avg;
    }
}
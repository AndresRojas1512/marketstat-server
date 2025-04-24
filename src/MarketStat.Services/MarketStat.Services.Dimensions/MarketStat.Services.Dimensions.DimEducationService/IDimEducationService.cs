using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEducationService;

public interface IDimEducationService
{
    Task<DimEducation> CreateEducationAsync(string specialization, string educationLevel);
    Task<DimEducation> GetEducationByIdAsync(int educationId);
    Task<IEnumerable<DimEducation>> GetAllEducationsAsync();
    Task<DimEducation> UpdateEducationAsync(int educationId, string specialization, string educationLevel);
    Task DeleteEducationAsync(int educationId);
}
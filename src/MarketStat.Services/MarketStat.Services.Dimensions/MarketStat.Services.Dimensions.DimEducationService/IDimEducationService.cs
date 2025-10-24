using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEducationService;

public interface IDimEducationService
{
    Task<DimEducation> CreateEducationAsync(string specialtyName, string specialtyCode, string educationLevelName);
    Task<DimEducation> GetEducationByIdAsync(int educationId);
    Task<IEnumerable<DimEducation>> GetAllEducationsAsync();
    Task<DimEducation> UpdateEducationAsync(int educationId, string specialtyName, string specialtyCode, string educationLevelName);
    Task DeleteEducationAsync(int educationId);
}
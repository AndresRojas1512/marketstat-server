using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEducationService;

public interface IDimEducationService
{
    Task<DimEducation> CreateEducationAsync(string specialty, string specialtyCode,int educationLevelId);
    Task<DimEducation> GetEducationByIdAsync(int educationId);
    Task<IEnumerable<DimEducation>> GetAllEducationsAsync();
    Task<DimEducation> UpdateEducationAsync(int educationId, string specialty, string specialtyCode, int educationLevelId);
    Task DeleteEducationAsync(int educationId);
}
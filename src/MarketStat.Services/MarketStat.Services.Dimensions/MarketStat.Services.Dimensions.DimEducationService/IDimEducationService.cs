using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Enums;

namespace MarketStat.Services.Dimensions.DimEducationService;

public interface IDimEducationService
{
    Task<DimEducation> CreateEducationAsync(string specialization, EducationLevel educationLevel, int industryFieldId);
    Task<DimEducation> GetEducationByIdAsync(int educationId);
    Task<IEnumerable<DimEducation>> GetAllEducationsAsync();
    Task<DimEducation> UpdateEducationAsync(int educationId, string specialization, EducationLevel educationLevel, int industryFieldId);
    Task DeleteEducationAsync(int educationId);
}
namespace MarketStat.Services.Dimensions.DimEducationService;

using MarketStat.Common.Core.Dimensions;

public interface IDimEducationService
{
    Task<DimEducation> CreateEducationAsync(string specialtyName, string specialtyCode, string educationLevelName);

    Task<DimEducation> GetEducationByIdAsync(int educationId);

    Task<IEnumerable<DimEducation>> GetAllEducationsAsync();

    Task<DimEducation> UpdateEducationAsync(int educationId, string specialtyName, string specialtyCode, string educationLevelName);

    Task DeleteEducationAsync(int educationId);
}

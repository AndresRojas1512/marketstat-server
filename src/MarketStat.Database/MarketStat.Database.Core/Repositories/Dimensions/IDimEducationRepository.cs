using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimEducationRepository
{
    Task AddEducationAsync(DimEducation education);
    Task<DimEducation> GetEducationByIdAsync(int educationId);
    Task<IEnumerable<DimEducation>> GetAllEducationsAsync();
    Task UpdateEducationAsync(DimEducation education);
    Task DeleteEducationAsync(int educationId);
}
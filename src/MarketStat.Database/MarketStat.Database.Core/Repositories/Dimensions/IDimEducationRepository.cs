namespace MarketStat.Database.Core.Repositories.Dimensions;

using MarketStat.Common.Core.Dimensions;

public interface IDimEducationRepository
{
    Task AddEducationAsync(DimEducation education);

    Task<DimEducation> GetEducationByIdAsync(int educationId);

    Task<IEnumerable<DimEducation>> GetAllEducationsAsync();

    Task UpdateEducationAsync(DimEducation education);

    Task DeleteEducationAsync(int educationId);
}

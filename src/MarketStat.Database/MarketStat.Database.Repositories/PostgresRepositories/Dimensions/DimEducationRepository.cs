using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEducationRepository
{
    private readonly Dictionary<int, DimEducation> _educations = new Dictionary<int, DimEducation>();
    
    public Task AddEducationAsync(DimEducation education)
    {
        if (!_educations.TryAdd(education.EducationId, education))
        {
            throw new ArgumentException($"Education {education.EducationId} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task<DimEducation> GetEducationByIdAsync(int educationId)
    {
        if (_educations.TryGetValue(educationId, out var e))
        {
            return Task.FromResult(e);
        }
        throw new KeyNotFoundException($"Employer {educationId} not found.");
    }

    public Task<IEnumerable<DimEducation>> GetAllEducationsAsync()
    {
        return Task.FromResult<IEnumerable<DimEducation>>(_educations.Values);
    }

    public Task UpdateEducationAsync(DimEducation education)
    {
        if (!_educations.ContainsKey(education.EducationId))
        {
            throw new KeyNotFoundException($"Cannot update: education {education.EducationId} not found.");
        }
        _educations[education.EducationId] = education;
        return Task.CompletedTask;
    }

    public Task DeleteEducationAsync(int educationId)
    {
        if (!_educations.ContainsKey(educationId))
        {
            throw new KeyNotFoundException($"Cannot delete: education {educationId} not found.");
        }
        return Task.CompletedTask;
    }
}
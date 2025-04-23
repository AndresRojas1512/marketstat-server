using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimIndustryFieldRepository : IDimIndustryFieldRepository
{
    private readonly Dictionary<int, DimIndustryField> _industryFields = new Dictionary<int, DimIndustryField>();
    
    public Task AddIndustryFieldAsync(DimIndustryField industryField)
    {
        if (!_industryFields.TryAdd(industryField.IndustryFieldId, industryField))
        {
            throw new ArgumentException($"IndustryField {industryField.IndustryFieldId} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task<DimIndustryField> GetIndustryFieldByIdAsync(int industryFieldId)
    {
        if (_industryFields.TryGetValue(industryFieldId, out var i))
        {
            return Task.FromResult(i);
        }
        throw new KeyNotFoundException($"IndustryField {industryFieldId} not found.");
    }

    public Task<IEnumerable<DimIndustryField>> GetAllIndustryFieldsAsync()
    {
        return Task.FromResult<IEnumerable<DimIndustryField>>(_industryFields.Values);
    }

    public Task UpdateIndustryFieldAsync(DimIndustryField industryField)
    {
        if (!_industryFields.ContainsKey(industryField.IndustryFieldId))
        {
            throw new KeyNotFoundException($"Cannot update: IndustryField {industryField.IndustryFieldId} not found.");
        }
        _industryFields[industryField.IndustryFieldId] = industryField;
        return Task.CompletedTask;
    }

    public Task DeleteIndustryFieldAsync(int industryFieldId)
    {
        if (!_industryFields.ContainsKey(industryFieldId))
        {
            throw new KeyNotFoundException($"Cannot delete: IndustryField {industryFieldId} not found.");
        }
        return Task.CompletedTask;
    }
}
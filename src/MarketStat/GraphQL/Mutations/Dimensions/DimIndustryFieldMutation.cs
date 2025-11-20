using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;
using MarketStat.Services.Dimensions.DimIndustryFieldService;

namespace MarketStat.GraphQL.Mutations.Dimensions;

[ExtendObjectType("Mutation")]
public class DimIndustryFieldMutation
{
    public async Task<DimIndustryFieldDto> CreateIndustryField(CreateDimIndustryFieldDto input,
        [Service] IDimIndustryFieldService industryFieldService, [Service] IMapper mapper)
    {
        var createdDomain =
            await industryFieldService.CreateIndustryFieldAsync(input.IndustryFieldCode, input.IndustryFieldName);
        return mapper.Map<DimIndustryFieldDto>(createdDomain);
    }

    public async Task<DimIndustryFieldDto> UpdateIndustryField(int id, UpdateDimIndustryFieldDto input,
        [Service] IDimIndustryFieldService industryFieldService, [Service] IMapper mapper)
    {
        var updatedDomain =
            await industryFieldService.UpdateIndustryFieldAsync(id, input.IndustryFieldCode, input.IndustryFieldName);
        return mapper.Map<DimIndustryFieldDto>(updatedDomain);
    }

    public async Task<bool> DeleteIndustryField(int id, [Service] IDimIndustryFieldService industryFieldService)
    {
        await industryFieldService.DeleteIndustryFieldAsync(id);
        return true;
    }
}
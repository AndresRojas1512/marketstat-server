using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;
using MarketStat.Services.Dimensions.DimDateService;

namespace MarketStat.GraphQL.Mutations.Dimensions;

[ExtendObjectType("Mutation")]
public class DimDateMutation
{
    public async Task<DimDateDto> CreateDate(CreateDimDateDto input, [Service] IDimDateService dateService,
        [Service] IMapper mapper)
    {
        var createdDomain = await dateService.CreateDateAsync(input.FullDate);
        return mapper.Map<DimDateDto>(createdDomain);
    }

    public async Task<DimDateDto> UpdateDate(int id, UpdateDimDateDto input, [Service] IDimDateService dateService,
        [Service] IMapper mapper)
    {
        var updatedDomain = await dateService.UpdateDateAsync(id, input.FullDate);
        return mapper.Map<DimDateDto>(updatedDomain);
    }

    public async Task<bool> DeleteDate(int id, [Service] IDimDateService dateService)
    {
        await dateService.DeleteDateAsync(id);
        return true;
    }
}
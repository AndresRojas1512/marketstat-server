using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducation;
using MarketStat.Services.Dimensions.DimEducationService;

namespace MarketStat.GraphQL.Mutations.Dimensions;

[ExtendObjectType("Mutation")]
public class DimEducationMutation
{
    public async Task<DimEducationDto> CreateEducation(CreateDimEducationDto input,
        [Service] IDimEducationService educationService, [Service] IMapper mapper)
    {
        var createDomain =
            await educationService.CreateEducationAsync(input.SpecialtyName, input.SpecialtyCode,
                input.EducationLevelName);
        return mapper.Map<DimEducationDto>(createDomain);
    }

    public async Task<DimEducationDto> UpdateEducation(int id, UpdateDimEducationDto input,
        [Service] IDimEducationService educationService, [Service] IMapper mapper)
    {
        var updateDomain = await educationService.UpdateEducationAsync(id, input.SpecialtyName, input.SpecialtyCode,
            input.EducationLevelName);
        return mapper.Map<DimEducationDto>(updateDomain);
    }

    public async Task<bool> DeleteEducation(int id, [Service] IDimEducationService educationService)
    {
        await educationService.DeleteEducationAsync(id);
        return true;
    }
}
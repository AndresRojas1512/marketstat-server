using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;

namespace MarketStat.GraphQL.Mutations.Dimensions;

[ExtendObjectType("Mutation")]
public class DimEmployerMutation
{
    public async Task<DimEmployerDto> CreateEmployer(CreateDimEmployerDto input,
        [Service] IDimEmployerService employerService, [Service] IMapper mapper)
    {
        var createdDomain = await employerService.CreateEmployerAsync(input.EmployerName, input.Inn, input.Ogrn,
            input.Kpp, input.RegistrationDate, input.LegalAddress, input.ContactEmail, input.ContactPhone,
            input.IndustryFieldId);
        return mapper.Map<DimEmployerDto>(createdDomain);
    }

    public async Task<DimEmployerDto> UpdateEmployer(int id, UpdateDimEmployerDto input,
        [Service] IDimEmployerService employerService, [Service] IMapper mapper)
    {
        var updatedDomain = await employerService.UpdateEmployerAsync(id, input.EmployerName, input.Inn, input.Ogrn,
            input.Kpp, input.RegistrationDate, input.LegalAddress, input.ContactEmail, input.ContactPhone,
            input.IndustryFieldId);
        return mapper.Map<DimEmployerDto>(updatedDomain);
    }

    public async Task<bool> DeleteEmployer(int id, [Service] IDimEmployerService employerService)
    {
        await employerService.DeleteEmployerAsync(id);
        return true;
    }
}
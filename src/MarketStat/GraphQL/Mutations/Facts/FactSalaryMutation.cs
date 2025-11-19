using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;
using MarketStat.Services.Facts.FactSalaryService;

namespace MarketStat.GraphQL.Mutations.Facts;

[ExtendObjectType("Mutation")]
public class FactSalaryMutation
{
    public async Task<FactSalaryDto> CreateSalaryFact(CreateFactSalaryDto input,
        [Service] IFactSalaryService salaryService, [Service] IMapper mapper)
    {
        var result = await salaryService.CreateFactSalaryAsync(input.DateId, input.LocationId, input.EmployerId,
            input.JobId, input.EmployeeId, input.SalaryAmount);
        return mapper.Map<FactSalaryDto>(result);
    }

    public async Task<FactSalaryDto> UpdateSalaryFact(long id, UpdateFactSalaryDto input,
        [Service] IFactSalaryService salaryService, [Service] IMapper mapper)
    {
        var result = await salaryService.UpdateFactSalaryAsync(id, input.DateId, input.LocationId, input.EmployerId,
            input.JobId, input.EmployeeId, input.SalaryAmount);
        return mapper.Map<FactSalaryDto>(result);
    }

    public async Task<bool> DeleteSalaryFact(long id, [Service] IFactSalaryService salaryService)
    {
        await salaryService.DeleteFactSalaryAsync(id);
        return true;
    }
}
using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployee;
using MarketStat.Services.Dimensions.DimEmployeeService;

namespace MarketStat.GraphQL.Queries.Dimensions;

[ExtendObjectType("Query")]
public class DimEmployeeQuery
{
    public async Task<DimEmployeeDto> GetEmployeeById(int id, [Service] IDimEmployeeService employeeService,
        [Service] IMapper mapper)
    {
        var domainResult = await employeeService.GetEmployeeByIdAsync(id);
        return mapper.Map<DimEmployeeDto>(domainResult);
    }

    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<DimEmployeeDto>> GetAllEmployees([Service] IDimEmployeeService employeeService,
        [Service] IMapper mapper)
    {
        var domainResult = await employeeService.GetAllEmployeesAsync();
        return mapper.Map<IEnumerable<DimEmployeeDto>>(domainResult);
    }
}
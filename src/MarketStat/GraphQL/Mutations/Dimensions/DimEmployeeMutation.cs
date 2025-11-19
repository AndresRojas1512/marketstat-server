using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployee;
using MarketStat.Services.Dimensions.DimEmployeeService;

namespace MarketStat.GraphQL.Mutations.Dimensions;

[ExtendObjectType("Mutation")]
public class DimEmployeeMutation
{
    public async Task<DimEmployeeDto> CreateEmployee(CreateDimEmployeeDto input,
        [Service] IDimEmployeeService employeeService, [Service] IMapper mapper)
    {
        var createdDomain = await employeeService.CreateEmployeeAsync(input.EmployeeRefId, input.BirthDate,
            input.CareerStartDate, input.Gender, input.EducationId, input.GraduationYear);
        return mapper.Map<DimEmployeeDto>(createdDomain);
    }

    public async Task<DimEmployeeDto> UpdateEmployee(int id, UpdateDimEmployeeDto input,
        [Service] IDimEmployeeService employeeService, [Service] IMapper mapper)
    {
        var updatedDomain = await employeeService.UpdateEmployeeAsync(id, input.EmployeeRefId, input.BirthDate,
            input.CareerStartDate, input.Gender, input.EducationId, input.GraduationYear);
        return mapper.Map<DimEmployeeDto>(updatedDomain);
    }

    public async Task<DimEmployeeDto> PatchEmployee(int id, PartialUpdateDimEmployeeDto input,
        [Service] IDimEmployeeService employeeService, [Service] IMapper mapper)
    {
        var updatedDomain = await employeeService.PartialUpdateEmployeeAsync(id, input.EmployeeRefId,
            input.CareerStartDate, input.EducationId, input.GraduationYear);
        return mapper.Map<DimEmployeeDto>(updatedDomain);
    }

    public async Task<bool> DeleteEmployee(int id, [Service] IDimEmployeeService employeeService)
    {
        await employeeService.DeleteEmployeeAsync(id);
        return true;
    }
}
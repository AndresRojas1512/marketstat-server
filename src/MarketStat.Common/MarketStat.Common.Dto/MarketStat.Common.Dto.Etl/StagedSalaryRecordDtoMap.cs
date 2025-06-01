using CsvHelper.Configuration;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Etl;

public sealed class StagedSalaryRecordDtoMap : ClassMap<StagedSalaryRecordDto>
{
    public StagedSalaryRecordDtoMap()
    {
        Map(m => m.RecordedDateText).Name("recorded_date_text");
        Map(m => m.CityName).Name("city_name");
        Map(m => m.OblastName).Name("oblast_name");
        Map(m => m.EmployerName).Name("employer_name");
        Map(m => m.StandardJobRoleTitle).Name("standard_job_role_title");
        Map(m => m.JobRoleTitle).Name("job_role_title");
        Map(m => m.HierarchyLevelName).Name("hierarchy_level_name");
        Map(m => m.EmployeeBirthDateText).Name("employee_birth_date_text");
        Map(m => m.EmployeeCareerStartDateText).Name("employee_career_start_date_text");
        Map(m => m.SalaryAmount).Name("salary_amount");
        Map(m => m.BonusAmount).Name("bonus_amount");
    }
}
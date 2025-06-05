using AutoMapper;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MarketStat.Services.Dimensions.DimCityService;
using MarketStat.Services.Dimensions.DimDateService;
using MarketStat.Services.Dimensions.DimEducationLevelService;
using MarketStat.Services.Dimensions.DimEducationService;
using MarketStat.Services.Dimensions.DimEmployeeEducationService;
using MarketStat.Services.Dimensions.DimEmployeeService;
using MarketStat.Services.Dimensions.DimEmployerIndustryFieldService;
using MarketStat.Services.Dimensions.DimEmployerService;
using MarketStat.Services.Dimensions.DimFederalDistrictService;
using MarketStat.Services.Dimensions.DimHierarchyLevelService;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using MarketStat.Services.Dimensions.DimJobRoleService;
using MarketStat.Services.Dimensions.DimOblastService;
using MarketStat.Services.Dimensions.DimStandardJobRoleHierarchyService;
using MarketStat.Services.Dimensions.DimStandardJobRoleService;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IntegrationTests.Services.AccessObject;

public class MarketStatAccessObjectInMemory : IDisposable
{
    public MarketStatDbContext Context { get; }
    public IMapper MockMapper { get; }
    public IDimEmployerRepository EmployerRepository { get; }
    public IDimIndustryFieldRepository IndustryFieldRepository { get; }
    public IDimJobRoleRepository JobRoleRepository { get; }
    public IDimDateRepository DimDateRepository { get; }
    public IDimEducationRepository DimEducationRepository { get; }
    public IDimEmployeeRepository DimEmployeeRepository { get; }
    public IDimEmployeeEducationRepository DimEmployeeEducationRepository { get; }
    public IDimHierarchyLevelRepository DimHierarchyLevelRepository { get; }
    public IDimEmployerIndustryFieldRepository DimEmployerIndustryFieldRepository { get; }
    public IDimFederalDistrictRepository DimFederalDistrictRepository { get; }
    public IDimOblastRepository DimOblastRepository { get; }
    public IDimCityRepository DimCityRepository { get; }
    public IDimEducationLevelRepository DimEducationLevelRepository { get; }
    public IDimStandardJobRoleHierarchyRepository DimStandardJobRoleHierarchyRepository { get; }
    public IDimStandardJobRoleRepository DimStandardJobRoleRepository { get; }
    
    public IFactSalaryRepository FactSalaryRepository { get; }
    
    public IUserRepository UserRepository { get;  }
    
    public IBenchmarkHistoryRepository BenchmarkHistoryRepository { get; }
    
    public IDimEmployerService EmployerService { get; }
    public IDimIndustryFieldService IndustryFieldService { get; }
    public IDimJobRoleService JobRoleService { get; }
    public IDimDateService DimDateService { get; }
    public IDimEducationService DimEducationService { get; }
    public IDimEmployeeService DimEmployeeService { get; }
    public IDimEmployeeEducationService DimEmployeeEducationService { get; }
    public IDimHierarchyLevelService DimHierarchyLevelService { get; }
    public IDimEmployerIndustryFieldService DimEmployerIndustryFieldService { get; }
    public IDimFederalDistrictService DimFederalDistrictService { get; }
    public IDimOblastService DimOblastService { get; }
    public IDimCityService DimCityService { get; }
    public IDimEducationLevelService DimEducationLevelService { get; }
    public IDimStandardJobRoleHierarchyService DimStandardJobRoleHierarchyService { get; }
    public IDimStandardJobRoleService DimStandardJobRoleService { get; }
    
    public IFactSalaryService FactSalaryService { get; }
    

    public MarketStatAccessObjectInMemory()
    {
        Context = new InMemoryDbContextFactory().GetDbContext();
        var mockMapper = new Mock<IMapper>();
        MockMapper = mockMapper.Object;
        
        EmployerRepository = new DimEmployerRepository(Context);
        EmployerService = new DimEmployerService(EmployerRepository, NullLogger<DimEmployerService>.Instance);
        
        IndustryFieldRepository = new DimIndustryFieldRepository(Context);
        IndustryFieldService = new DimIndustryFieldService(IndustryFieldRepository, NullLogger<DimIndustryFieldService>.Instance);
        
        JobRoleRepository = new DimJobRoleRepository(Context);
        JobRoleService = new DimJobRoleService(JobRoleRepository, NullLogger<DimJobRoleService>.Instance);
        
        DimDateRepository = new DimDateRepository(Context);
        DimDateService = new DimDateService(DimDateRepository, NullLogger<DimDateService>.Instance);
        
        DimEducationRepository = new DimEducationRepository(Context);
        DimEducationService = new DimEducationService(DimEducationRepository, NullLogger<DimEducationService>.Instance);
        
        DimEmployeeRepository = new DimEmployeeRepository(Context);
        DimEmployeeService = new DimEmployeeService(DimEmployeeRepository, NullLogger<DimEmployeeService>.Instance);

        DimEmployeeEducationRepository = new DimEmployeeEducationRepository(Context);
        DimEmployeeEducationService = new DimEmployeeEducationService(DimEmployeeEducationRepository, 
            NullLogger<DimEmployeeEducationService>.Instance);
        
        DimHierarchyLevelRepository = new DimHierarchyLevelRepository(Context);
        DimHierarchyLevelService =
            new DimHierarchyLevelService(DimHierarchyLevelRepository, NullLogger<DimHierarchyLevelService>.Instance);

        DimEmployerIndustryFieldRepository = new DimEmployerIndustryFieldRepository(Context);
        DimEmployerIndustryFieldService = new DimEmployerIndustryFieldService(DimEmployerIndustryFieldRepository,
            NullLogger<DimEmployerIndustryFieldService>.Instance);
        
        DimFederalDistrictRepository = new DimFederalDistrictRepository(Context);
        DimFederalDistrictService = new DimFederalDistrictService(DimFederalDistrictRepository,
            NullLogger<DimFederalDistrictService>.Instance);
        
        DimOblastRepository = new DimOblastRepository(Context);
        DimOblastService = new DimOblastService(DimOblastRepository, NullLogger<DimOblastService>.Instance);
        
        DimCityRepository = new DimCityRepository(Context);
        DimCityService = new DimCityService(DimCityRepository, NullLogger<DimCityService>.Instance);

        DimEducationLevelRepository = new DimEducationLevelRepository(Context);
        DimEducationLevelService =
            new DimEducationLevelService(DimEducationLevelRepository, NullLogger<DimEducationLevelService>.Instance);
        
        DimStandardJobRoleHierarchyRepository = new DimStandardJobRoleHierarchyRepository(Context);
        DimStandardJobRoleHierarchyService = new DimStandardJobRoleHierarchyService(
            DimStandardJobRoleHierarchyRepository, NullLogger<DimStandardJobRoleHierarchyService>.Instance);

        DimStandardJobRoleRepository = new DimStandardJobRoleRepository(Context);
        DimStandardJobRoleService = new DimStandardJobRoleService(DimStandardJobRoleRepository,
            NullLogger<DimStandardJobRoleService>.Instance);
        
        FactSalaryRepository = new FactSalaryRepository(Context, NullLogger<FactSalaryRepository>.Instance);
        FactSalaryService = new FactSalaryService(FactSalaryRepository, MockMapper, NullLogger<FactSalaryService>.Instance);
    }

    public async Task SeedEmployerAsync(IEnumerable<DimEmployer> items)
    {
        foreach (var e in items)
        {
            Context.DimEmployers.Add(DimEmployerConverter.ToDbModel(e));
        }
        await Context.SaveChangesAsync();
    }

    public async Task SeedIndustryFieldAsync(IEnumerable<DimIndustryField> items)
    {
        foreach (var i in items)
        {
            Context.DimIndustryFields.Add(DimIndustryFieldConverter.ToDbModel(i));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedJobRoleAsync(IEnumerable<DimJobRole> items)
    {
        foreach (var j in items)
        {
            Context.DimJobRoles.Add(DimJobRoleConverter.ToDbModel(j));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedDateAsync(IEnumerable<DimDate> items)
    {
        foreach (var d in items)
        {
            Context.DimDates.Add(DimDateConverter.ToDbModel(d));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedEducationAsync(IEnumerable<DimEducation> items)
    {
        foreach (var e in items)
        {
            Context.DimEducations.Add(DimEducationConverter.ToDbModel(e));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedEmployeeAsync(IEnumerable<DimEmployee> items)
    {
        foreach (var e in items)
        {
            Context.DimEmployees.Add(DimEmployeeConverter.ToDbModel(e));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedEmployeeEducationsAsync(IEnumerable<DimEmployeeEducation> items)
    {
        foreach (var ee in items)
            Context.DimEmployeeEducations.Add(
                DimEmployeeEducationConverter.ToDbModel(ee)
            );
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedHierarchyLevelsAsync(IEnumerable<DimHierarchyLevel> items)
    {
        foreach (var h in items)
        {
            Context.DimHierarchyLevels.Add(DimHierarchyLevelConverter.ToDbModel(h));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedEmployerIndustryFieldAsync(IEnumerable<DimEmployerIndustryField> items)
    {
        foreach (var ei in items)
            Context.DimEmployerIndustryFields.Add(
                DimEmployerIndustryFieldConverter.ToDbModel(ei)
            );
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedFederalDistrictAsync(IEnumerable<DimFederalDistrict> items)
    {
        foreach (var f in items)
        {
            Context.DimFederalDistricts.Add(DimFederalDistrictConverter.ToDbModel(f));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedOblastAsync(IEnumerable<DimOblast> items)
    {
        foreach (var o in items)
        {
            Context.DimOblasts.Add(DimOblastConverter.ToDbModel(o));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedCityAsync(IEnumerable<DimCity> items)
    {
        foreach (var c in items)
        {
            Context.DimCities.Add(DimCityConverter.ToDbModel(c));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedEducationLevelAsync(IEnumerable<DimEducationLevel> items)
    {
        foreach (var e in items)
        {
            Context.DimEducationLevels.Add(DimEducationLevelConverter.ToDbModel(e));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedStandardJobRoleHierarchyAsync(IEnumerable<DimStandardJobRoleHierarchy> items)
    {
        foreach (var j in items)
        {
            Context.DimStandardJobRoleHierarchies.Add(DimStandardJobRoleHierarchyConverter.ToDbModel(j));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedStandardJobRoleAsync(IEnumerable<DimStandardJobRole> items)
    {
        foreach (var j in items)
        {
            Context.DimStandardJobRoles.Add(DimStandardJobRoleConverter.ToDbModel(j));
        }
        await Context.SaveChangesAsync();
    }
    
    public async Task SeedSalaryAsync(IEnumerable<FactSalary> items)
    {
        foreach (var s in items)
        {
            Context.FactSalaries.Add(FactSalaryConverter.ToDbModel(s));
        }
        await Context.SaveChangesAsync();
    }
    
    public void Dispose() => Context.Dispose();
}
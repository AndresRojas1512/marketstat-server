using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Services.Dimensions.DimDateService;
using MarketStat.Services.Dimensions.DimEducationService;
using MarketStat.Services.Dimensions.DimEmployeeEducationService;
using MarketStat.Services.Dimensions.DimEmployeeService;
using MarketStat.Services.Dimensions.DimEmployerIndustryFieldService;
using MarketStat.Services.Dimensions.DimEmployerService;
using MarketStat.Services.Dimensions.DimHierarchyLevelService;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using MarketStat.Services.Dimensions.DimJobRoleService;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntegrationTests.Services.AccessObject;

public class MarketStatAccessObjectInMemory : IDisposable
{
    public MarketStatDbContext Context { get; }
    public IDimEmployerRepository EmployerRepository { get; }
    public IDimIndustryFieldRepository IndustryFieldRepository { get; }
    public IDimJobRoleRepository JobRoleRepository { get; }
    public IDimDateRepository DimDateRepository { get; }
    public IDimEducationRepository DimEducationRepository { get; }
    public IDimEmployeeRepository DimEmployeeRepository { get; }
    public IDimEmployeeEducationRepository DimEmployeeEducationRepository { get; }
    public IDimHierarchyLevelRepository DimHierarchyLevelRepository { get; }
    public IDimEmployerIndustryFieldRepository DimEmployerIndustryFieldRepository { get; }
    
    public IDimEmployerService EmployerService { get; }
    public IDimIndustryFieldService IndustryFieldService { get; }
    public IDimJobRoleService JobRoleService { get; }
    public IDimDateService DimDateService { get; }
    public IDimEducationService DimEducationService { get; }
    public IDimEmployeeService DimEmployeeService { get; }
    public IDimEmployeeEducationService DimEmployeeEducationService { get; }
    public IDimHierarchyLevelService DimHierarchyLevelService { get; }
    public IDimEmployerIndustryFieldService DimEmployerIndustryFieldService { get; }

    public MarketStatAccessObjectInMemory()
    {
        Context = new InMemoryDbContextFactory().GetDbContext();
        
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
    
    public void Dispose() => Context.Dispose();
}
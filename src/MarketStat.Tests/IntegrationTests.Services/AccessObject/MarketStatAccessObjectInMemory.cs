using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Services.Dimensions.DimDateService;
using MarketStat.Services.Dimensions.DimEducationService;
using MarketStat.Services.Dimensions.DimEmployerService;
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
    
    public IDimEmployerService EmployerService { get; }
    public IDimIndustryFieldService IndustryFieldService { get; }
    public IDimJobRoleService JobRoleService { get; }
    public IDimDateService DimDateService { get; }
    public IDimEducationService DimEducationService { get; }

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
    
    public void Dispose() => Context.Dispose();
}
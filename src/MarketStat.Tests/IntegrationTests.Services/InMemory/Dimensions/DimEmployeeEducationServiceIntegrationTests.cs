using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployeeEducationService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEmployeeEducationServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEmployeeEducationService _dimEmployeeEducationService;

    public DimEmployeeEducationServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEmployeeEducationService = new DimEmployeeEducationService(_accessObject.DimEmployeeEducationRepository,
            NullLogger<DimEmployeeEducationService>.Instance);
    }
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task CreateEmployeeEducationAsync_ValidParameters_CreatesAndReturnsLink()
    {
        var created = await _dimEmployeeEducationService.CreateEmployeeEducationAsync(1, 2, 2022);

        Assert.Equal(1, created.EmployeeId);
        Assert.Equal(2, created.EducationId);
        Assert.Equal(2022, created.GraduationYear);

        var all = (await _dimEmployeeEducationService.GetAllEmployeeEducationsAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(created.EmployeeId, all[0].EmployeeId);
        Assert.Equal(created.EducationId, all[0].EducationId);
    }
    
    [Fact]
    public async Task CreateEmployeeEducationAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeEducationService.CreateEmployeeEducationAsync(0, 1, 2020));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeEducationService.CreateEmployeeEducationAsync(1, 0, 2020));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeEducationService.CreateEmployeeEducationAsync(1, 1, (short)0));
    }
    
    [Fact]
    public async Task GetEmployeeEducationAsync_Existing_ReturnsLink()
    {
        var seed = new DimEmployeeEducation(3, 4, 2019);
        await _accessObject.SeedEmployeeEducationsAsync(new[] { seed });

        var fetched = await _dimEmployeeEducationService.GetEmployeeEducationAsync(3, 4);

        Assert.Equal(seed.EmployeeId, fetched.EmployeeId);
        Assert.Equal(seed.EducationId, fetched.EducationId);
        Assert.Equal(seed.GraduationYear, fetched.GraduationYear);
    }
    
    [Fact]
    public async Task GetEmployeeEducationAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeEducationService.GetEmployeeEducationAsync(99, 99));
        Assert.Contains("was not found", ex.Message);
    }
    
    [Fact]
    public async Task GetEducationsByEmployeeIdAsync_WithLinks_ReturnsOnlyThatEmployee()
    {
        var seed = new[]
        {
            new DimEmployeeEducation(5, 1, 2018),
            new DimEmployeeEducation(5, 2, 2017),
            new DimEmployeeEducation(6, 3, 2016)
        };
        await _accessObject.SeedEmployeeEducationsAsync(seed);

        var list5 = (await _dimEmployeeEducationService.GetEducationsByEmployeeIdAsync(5)).ToList();
        Assert.Equal(2, list5.Count);
        Assert.All(list5, e => Assert.Equal(5, e.EmployeeId));

        var list6 = (await _dimEmployeeEducationService.GetEducationsByEmployeeIdAsync(6)).ToList();
        Assert.Single(list6);
        Assert.Equal(6, list6[0].EmployeeId);
    }
    
    [Fact]
    public async Task GetEducationsByEmployeeIdAsync_NoLinks_ReturnsEmpty()
    {
        var empty = await _dimEmployeeEducationService.GetEducationsByEmployeeIdAsync(42);
        Assert.Empty(empty);
    }
    
    [Fact]
    public async Task GetAllEmployeeEducationsAsync_Seeded_ReturnsAllLinks()
    {
        var seed = new[]
        {
            new DimEmployeeEducation(7, 7, 2015),
            new DimEmployeeEducation(8, 8, 2014),
        };
        await _accessObject.SeedEmployeeEducationsAsync(seed);

        var all = (await _dimEmployeeEducationService.GetAllEmployeeEducationsAsync()).ToList();
        Assert.Equal(2, all.Count);
        Assert.Contains(all, e => e.EmployeeId == 7 && e.EducationId == 7);
        Assert.Contains(all, e => e.EmployeeId == 8 && e.EducationId == 8);
    }
    
    [Fact]
    public async Task UpdateEmployeeEducationAsync_Existing_UpdatesGraduationYear()
    {
        var original = new DimEmployeeEducation(9, 9, 2010);
        await _accessObject.SeedEmployeeEducationsAsync(new[] { original });

        var updated = await _dimEmployeeEducationService.UpdateEmployeeEducationAsync(9, 9, 2023);

        Assert.Equal(2023, updated.GraduationYear);

        var fetched = await _dimEmployeeEducationService.GetEmployeeEducationAsync(9, 9);
        Assert.Equal(2023, fetched.GraduationYear);
    }
    
    [Fact]
    public async Task UpdateEmployeeEducationAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeEducationService.UpdateEmployeeEducationAsync(100, 100, 2000));
        Assert.Contains("was not found", ex.Message);
    }
    
    [Fact]
    public async Task DeleteEmployeeEducationAsync_Existing_RemovesLink()
    {
        var seed = new DimEmployeeEducation(10, 10, 2005);
        await _accessObject.SeedEmployeeEducationsAsync(new[] { seed });

        await _dimEmployeeEducationService.DeleteEmployeeEducationAsync(10, 10);

        var all = (await _dimEmployeeEducationService.GetAllEmployeeEducationsAsync()).ToList();
        Assert.DoesNotContain(all, e => e.EmployeeId == 10 && e.EducationId == 10);
    }
    
    [Fact]
    public async Task DeleteEmployeeEducationAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeEducationService.DeleteEmployeeEducationAsync(123, 456));
        Assert.Contains("Could not remove education", ex.Message);
    }
}
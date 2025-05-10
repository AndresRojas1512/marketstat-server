using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Services.Dimensions.DimEducationService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEducationServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEducationService _dimEducationService;

    public DimEducationServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEducationService = new DimEducationService(_accessObject.DimEducationRepository,
            NullLogger<DimEducationService>.Instance);
    }
    
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllEducationsAsync_Empty_ReturnsEmpty()
    {
        var all = await _dimEducationService.GetAllEducationsAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllEducationsAsync_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimEducation>
        {
            new DimEducation(1, "Computer Science", "CS101", 1, 10),
            new DimEducation(2, "Mathematics",      "MATH1", 2, 20)
        };
        await _accessObject.SeedEducationAsync(seed);

        var all = (await _dimEducationService.GetAllEducationsAsync()).ToList();

        Assert.Equal(2, all.Count);
        Assert.Contains(all, e => e.Specialty     == "Computer Science"
                                  && e.SpecialtyCode == "CS101"
                                  && e.EducationLevelId == 1
                                  && e.IndustryFieldId  == 10);
        Assert.Contains(all, e => e.Specialty     == "Mathematics"
                                  && e.SpecialtyCode == "MATH1"
                                  && e.EducationLevelId == 2
                                  && e.IndustryFieldId  == 20);
    }
    
    [Fact]
    public async Task CreateEducationAsync_Valid_ReturnsCreated()
    {
        var created = await _dimEducationService.CreateEducationAsync(
            "Physics", "PHY1", educationLevelId: 3, industryFieldId: 30);

        Assert.NotNull(created);
        Assert.True(created.EducationId > 0);
        Assert.Equal("Physics",     created.Specialty);
        Assert.Equal("PHY1",        created.SpecialtyCode);
        Assert.Equal(3,             created.EducationLevelId);
        Assert.Equal(30,            created.IndustryFieldId);

        var fromDb = await _dimEducationService.GetEducationByIdAsync(created.EducationId);
        Assert.Equal(created.EducationId, fromDb.EducationId);
    }
    
    [Fact]
    public async Task GetEducationByIdAsync_Existing_ReturnsEducation()
    {
        var seed = new DimEducation(5, "Biology", "BIO1", 4, 40);
        await _accessObject.SeedEducationAsync(new[] { seed });

        var fetched = await _dimEducationService.GetEducationByIdAsync(5);

        Assert.Equal(5,       fetched.EducationId);
        Assert.Equal("Biology",  fetched.Specialty);
        Assert.Equal("BIO1",     fetched.SpecialtyCode);
        Assert.Equal(4,          fetched.EducationLevelId);
        Assert.Equal(40,         fetched.IndustryFieldId);
    }
    
    [Fact]
    public async Task GetEducationByIdAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEducationService.GetEducationByIdAsync(99));
    }
    
    [Fact]
    public async Task UpdateEducationAsync_Valid_ReturnsUpdated()
    {
        var seed = new DimEducation(7, "History", "HIS1", 5, 50);
        await _accessObject.SeedEducationAsync(new[] { seed });

        var updated = await _dimEducationService.UpdateEducationAsync(
            educationId: 7,
            specialty:      "World History",
            specialtyCode:  "WHIS",
            educationLevelId: 6,
            industryFieldId:  60
        );

        Assert.Equal(7,          updated.EducationId);
        Assert.Equal("World History", updated.Specialty);
        Assert.Equal("WHIS",         updated.SpecialtyCode);
        Assert.Equal(6,              updated.EducationLevelId);
        Assert.Equal(60,             updated.IndustryFieldId);
    }
    
    [Fact]
    public async Task UpdateEducationAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEducationService.UpdateEducationAsync(
                educationId: 123,
                specialty:      "X",
                specialtyCode:  "X",
                educationLevelId: 1,
                industryFieldId:  1
            ));
    }
    
    [Fact]
    public async Task DeleteEducationAsync_Existing_Completes()
    {
        var seed = new DimEducation(9, "Chemistry", "CHEM", 7, 70);
        await _accessObject.SeedEducationAsync(new[] { seed });

        await _dimEducationService.DeleteEducationAsync(9);

        var remaining = await _dimEducationService.GetAllEducationsAsync();
        Assert.DoesNotContain(remaining, e => e.EducationId == 9);
    }
    
    [Fact]
    public async Task DeleteEducationAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEducationService.DeleteEducationAsync(999));
    }
}
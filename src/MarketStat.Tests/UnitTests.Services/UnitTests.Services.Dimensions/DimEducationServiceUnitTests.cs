using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEducationService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimEducationServiceUnitTests
{
    private readonly Mock<IDimEducationRepository> _dimEducationRepositoryMock;
    private readonly Mock<ILogger<DimEducationService>> _loggerMock;
    private readonly DimEducationService _dimEducationService;

    public DimEducationServiceUnitTests()
    {
        _dimEducationRepositoryMock = new Mock<IDimEducationRepository>();
        _loggerMock = new Mock<ILogger<DimEducationService>>();
        _dimEducationService = new DimEducationService(_dimEducationRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateEducationAsync_ValidParameters_ReturnsNewDimEducation()
    {
        _dimEducationRepositoryMock
            .Setup(r => r.GetAllEducationsAsync())
            .ReturnsAsync(new List<DimEducation>());
        _dimEducationRepositoryMock
            .Setup(r => r.AddEducationAsync(It.IsAny<DimEducation>()))
            .Returns(Task.CompletedTask);

        var result = await _dimEducationService.CreateEducationAsync(
            "Computer Science", 1, 2
        );

        Assert.Equal(1, result.EducationId);
        Assert.Equal("Computer Science", result.Specialization);
        Assert.Equal(1, result.EducationLevelId);
        Assert.Equal(2, result.IndustryFieldId);

        _dimEducationRepositoryMock.Verify(r => r.AddEducationAsync(
            It.Is<DimEducation>(e =>
                e.EducationId     == 1 &&
                e.Specialization  == "Computer Science" &&
                e.EducationLevelId == 1 &&
                e.IndustryFieldId == 2
            )), Times.Once);
    }
    
    [Fact]
    public async Task CreateEducationAsync_Duplicate_ThrowsException()
    {
        _dimEducationRepositoryMock
            .Setup(r => r.GetAllEducationsAsync())
            .ReturnsAsync(new List<DimEducation>());
        _dimEducationRepositoryMock
            .Setup(r => r.AddEducationAsync(It.IsAny<DimEducation>()))
            .ThrowsAsync(new InvalidOperationException("duplicate"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationService.CreateEducationAsync("Math", 3, 3)
        );
        Assert.Equal("An education record with ID 1 already exists.", ex.Message);
    }
    
    [Fact]
    public async Task CreateEducationAsync_NullSpecialization_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.CreateEducationAsync(null!, 1, 1)
        );
    }
    
    [Fact]
    public async Task GetEducationByIdAsync_Existing_ReturnsDimEducation()
    {
        var expected = new DimEducation(5, "Bio", 1, 1);
        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(5))
            .ReturnsAsync(expected);

        var actual = await _dimEducationService.GetEducationByIdAsync(5);

        Assert.Same(expected, actual);
    }
    
    [Fact]
    public async Task GetEducationByIdAsync_NotFound_ThrowsException()
    {
        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(7))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationService.GetEducationByIdAsync(7)
        );
        Assert.Equal("Education with ID 7 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetAllEducationsAsync_ReturnsList()
    {
        var list = new List<DimEducation>
        {
            new DimEducation(1, "CS", 1, 1),
            new DimEducation(2, "EE", 2, 2)
        };
        _dimEducationRepositoryMock
            .Setup(r => r.GetAllEducationsAsync())
            .ReturnsAsync(list);

        var result = (await _dimEducationService.GetAllEducationsAsync()).ToList();
        
        Assert.Equal(2, result.Count);
        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task UpdateEducationAsync_ValidParameters_UpdatesAndReturns()
    {
        var existing = new DimEducation(3, "Eng", 1, 1);
        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(3))
            .ReturnsAsync(existing);
        _dimEducationRepositoryMock
            .Setup(r => r.UpdateEducationAsync(It.IsAny<DimEducation>()))
            .Returns(Task.CompletedTask);

        var updated = await _dimEducationService.UpdateEducationAsync(3, "Engineering", 2, 2);

        Assert.Equal(3, updated.EducationId);
        Assert.Equal("Engineering", updated.Specialization);
        Assert.Equal(2, updated.EducationLevelId);
        Assert.Equal(2, updated.IndustryFieldId);

        _dimEducationRepositoryMock.Verify(r => r.UpdateEducationAsync(
            It.Is<DimEducation>(e =>
                e.EducationId     == 3 &&
                e.Specialization  == "Engineering" &&
                e.EducationLevelId == 2 &&
                e.IndustryFieldId == 2
            )), Times.Once);
    }
    
    [Fact]
    public async Task UpdateEducationAsync_NotFound_ThrowsException()
    {
        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(9))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationService.UpdateEducationAsync(9, "X", 1, 1)
        );
        Assert.Equal("Cannot update: education 9 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task UpdateEducationAsync_InvalidId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.UpdateEducationAsync(0, "History", 1, 1)
        );
    }
    
    [Fact]
    public async Task UpdateEducationAsync_EmptySpecialization_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.UpdateEducationAsync(1, "", 1, 1)
        );
    }
    
    [Fact]
    public async Task UpdateEducationAsync_InvalidEducationLevel_ThrowsArgument()
    {
        var invalidLevel = -1;
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.UpdateEducationAsync(1, "History", invalidLevel, 1));
    }
    
    [Fact]
    public async Task UpdateEducationAsync_InvalidIndustryFieldId_ThrowsArgument()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.UpdateEducationAsync(1, "History", 1, 0));
    }
    
    [Fact]
    public async Task DeleteEducationAsync_Existing_Completes()
    {
        _dimEducationRepositoryMock
            .Setup(r => r.DeleteEducationAsync(4))
            .Returns(Task.CompletedTask);

        await _dimEducationService.DeleteEducationAsync(4);

        _dimEducationRepositoryMock.Verify(r => r.DeleteEducationAsync(4), Times.Once);
    }
    
    [Fact]
    public async Task DeleteEducationAsync_NotFound_ThrowsException()
    {
        _dimEducationRepositoryMock
            .Setup(r => r.DeleteEducationAsync(6))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationService.DeleteEducationAsync(6)
        );
        Assert.Equal("Cannot delete: education 6 not found.", ex.Message);
    }
    
}
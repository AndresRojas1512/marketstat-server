using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEducationLevelService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimEducationLevelServiceUnitTests
{
    private readonly Mock<IDimEducationLevelRepository> _dimEducationLevelRepositoryMock;
    private readonly Mock<ILogger<DimEducationLevelService>> _loggerMock;
    private readonly DimEducationLevelService _dimEducationLevelService;

    public DimEducationLevelServiceUnitTests()
    {
        _dimEducationLevelRepositoryMock = new Mock<IDimEducationLevelRepository>();
        _loggerMock = new Mock<ILogger<DimEducationLevelService>>();
        _dimEducationLevelService =
            new DimEducationLevelService(_dimEducationLevelRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateEducationLevelAsync_ValidParameters_ReturnsNewLevel()
    {
        _dimEducationLevelRepositoryMock
            .Setup(r => r.AddEducationLevelAsync(It.IsAny<DimEducationLevel>()))
            .Callback<DimEducationLevel>(lvl => lvl.EducationLevelId = 1)
            .Returns(Task.CompletedTask);

        var result = await _dimEducationLevelService.CreateEducationLevelAsync("Diploma");

        Assert.Equal(1, result.EducationLevelId);
        Assert.Equal("Diploma", result.EducationLevelName);
        _dimEducationLevelRepositoryMock.Verify(r =>
            r.AddEducationLevelAsync(
                It.Is<DimEducationLevel>(lvl =>
                    lvl.EducationLevelId   == 1 &&
                    lvl.EducationLevelName == "Diploma"
                )), Times.Once);
    }
    
    [Fact]
    public async Task CreateEducationLevelAsync_Duplicate_ThrowsException()
    {
        _dimEducationLevelRepositoryMock
            .Setup(r => r.AddEducationLevelAsync(It.IsAny<DimEducationLevel>()))
            .Callback<DimEducationLevel>(lvl => lvl.EducationLevelId = 1)
            .ThrowsAsync(new Exception("db error"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationLevelService.CreateEducationLevelAsync("Certificate"));

        Assert.Equal("Could not create DimEducationLevel 1.", ex.Message);
    }
    
    [Fact]
    public async Task GetEducationLevelByIdAsync_Existing_ReturnsLevel()
    {
        var expected = new DimEducationLevel(5, "PhD");
        _dimEducationLevelRepositoryMock
            .Setup(r => r.GetEducationLevelByIdAsync(5))
            .ReturnsAsync(expected);

        var actual = await _dimEducationLevelService.GetEducationLevelByIdAsync(5);

        Assert.Same(expected, actual);
    }
    
    [Fact]
    public async Task GetEducationLevelByIdAsync_NotFound_ThrowsException()
    {
        _dimEducationLevelRepositoryMock
            .Setup(r => r.GetEducationLevelByIdAsync(7))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationLevelService.GetEducationLevelByIdAsync(7));

        Assert.Equal("EducationLevel 7 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetAllEducationLevelsAsync_ReturnsList()
    {
        var list = new List<DimEducationLevel>
        {
            new DimEducationLevel(1, "Bachelors"),
            new DimEducationLevel(2, "Masters")
        };
        _dimEducationLevelRepositoryMock
            .Setup(r => r.GetAllEducationLevelsAsync())
            .ReturnsAsync(list);

        var result = (await _dimEducationLevelService.GetAllEducationLevelsAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task UpdateEducationLevelAsync_ValidParameters_UpdatesAndReturns()
    {
        var existing = new DimEducationLevel(3, "OldName");
        _dimEducationLevelRepositoryMock
            .Setup(r => r.GetEducationLevelByIdAsync(3))
            .ReturnsAsync(existing);
        _dimEducationLevelRepositoryMock
            .Setup(r => r.UpdateEducationLevelAsync(It.IsAny<DimEducationLevel>()))
            .Returns(Task.CompletedTask);

        var updated = await _dimEducationLevelService.UpdateEducationLevelAsync(3, "NewName");

        Assert.Equal(3, updated.EducationLevelId);
        Assert.Equal("NewName", updated.EducationLevelName);
        _dimEducationLevelRepositoryMock.Verify(r =>
            r.UpdateEducationLevelAsync(
                It.Is<DimEducationLevel>(lvl =>
                    lvl.EducationLevelId   == 3 &&
                    lvl.EducationLevelName == "NewName"
                )), Times.Once);
    }
    
    [Fact]
    public async Task UpdateEducationLevelAsync_NotFound_ThrowsException()
    {
        _dimEducationLevelRepositoryMock
            .Setup(r => r.GetEducationLevelByIdAsync(9))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationLevelService.UpdateEducationLevelAsync(9, "X"));

        Assert.Equal("Cannot update: education level 9 not found.", ex.Message);
    }
    
    [Fact]
    public async Task DeleteEducationLevelAsync_ValidId_CallsRepository()
    {
        _dimEducationLevelRepositoryMock
            .Setup(r => r.DeleteEducationLevelAsync(4))
            .Returns(Task.CompletedTask);

        await _dimEducationLevelService.DeleteEducationLevelAsync(4);

        _dimEducationLevelRepositoryMock.Verify(r =>
            r.DeleteEducationLevelAsync(4), Times.Once);
    }
    
    [Fact]
    public async Task DeleteEducationLevelAsync_NotFound_ThrowsException()
    {
        _dimEducationLevelRepositoryMock
            .Setup(r => r.DeleteEducationLevelAsync(6))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationLevelService.DeleteEducationLevelAsync(6));

        Assert.Equal("Cannot delete: education level 6 not found.", ex.Message);
    }
}
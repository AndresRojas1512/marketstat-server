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
        // Arrange
        _dimEducationRepositoryMock
            .Setup(r => r.GetAllEducationsAsync())
            .ReturnsAsync(new List<DimEducation>());
        _dimEducationRepositoryMock
            .Setup(r => r.AddEducationAsync(It.IsAny<DimEducation>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _dimEducationService.CreateEducationAsync(
            "Computer Science", "Master"
        );

        // Assert
        Assert.Equal(1, result.EducationId);
        Assert.Equal("Computer Science", result.Specialization);
        Assert.Equal("Master", result.EducationLevel);

        _dimEducationRepositoryMock.Verify(r => r.AddEducationAsync(
            It.Is<DimEducation>(e =>
                e.EducationId     == 1 &&
                e.Specialization  == "Computer Science" &&
                e.EducationLevel == "Master"
            )), Times.Once);
    }
    
    [Fact]
    public async Task CreateEducationAsync_Duplicate_ThrowsException()
    {
        // Arrange
        _dimEducationRepositoryMock
            .Setup(r => r.GetAllEducationsAsync())
            .ReturnsAsync(new List<DimEducation>());
        _dimEducationRepositoryMock
            .Setup(r => r.AddEducationAsync(It.IsAny<DimEducation>()))
            .ThrowsAsync(new InvalidOperationException("duplicate"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationService.CreateEducationAsync("Math", "PhD")
        );
        Assert.Equal("An education record with ID 1 already exists.", ex.Message);
    }
    
    [Fact]
    public async Task CreateEducationAsync_NullSpecialization_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.CreateEducationAsync(null!, "Bachelor")
        );
    }
    
    [Fact]
    public async Task CreateEducationAsync_NullEducationLevel_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.CreateEducationAsync("History", null!)
        );
    }
    
    [Fact]
    public async Task GetEducationByIdAsync_Existing_ReturnsDimEducation()
    {
        // Arrange
        var expected = new DimEducation(5, "Bio", "Bachelor");
        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(5))
            .ReturnsAsync(expected);

        // Act
        var actual = await _dimEducationService.GetEducationByIdAsync(5);

        // Assert
        Assert.Same(expected, actual);
    }
    
    [Fact]
    public async Task GetEducationByIdAsync_NotFound_ThrowsException()
    {
        // Arrange
        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(7))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationService.GetEducationByIdAsync(7)
        );
        Assert.Equal("Education with ID 7 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetAllEducationsAsync_ReturnsList()
    {
        // Arrange
        var list = new List<DimEducation>
        {
            new DimEducation(1, "CS", "BSc"),
            new DimEducation(2, "EE", "MSc")
        };
        _dimEducationRepositoryMock
            .Setup(r => r.GetAllEducationsAsync())
            .ReturnsAsync(list);

        // Act
        var result = (await _dimEducationService.GetAllEducationsAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task UpdateEducationAsync_ValidParameters_UpdatesAndReturns()
    {
        // Arrange
        var existing = new DimEducation(3, "Eng", "BEng");
        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(3))
            .ReturnsAsync(existing);
        _dimEducationRepositoryMock
            .Setup(r => r.UpdateEducationAsync(It.IsAny<DimEducation>()))
            .Returns(Task.CompletedTask);

        // Act
        var updated = await _dimEducationService.UpdateEducationAsync(3, "Engineering", "MEng");

        // Assert
        Assert.Equal(3, updated.EducationId);
        Assert.Equal("Engineering", updated.Specialization);
        Assert.Equal("MEng", updated.EducationLevel);

        _dimEducationRepositoryMock.Verify(r => r.UpdateEducationAsync(
            It.Is<DimEducation>(e =>
                e.EducationId     == 3 &&
                e.Specialization  == "Engineering" &&
                e.EducationLevel == "MEng"
            )), Times.Once);
    }
    
    [Fact]
    public async Task UpdateEducationAsync_NotFound_ThrowsException()
    {
        // Arrange
        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(9))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationService.UpdateEducationAsync(9, "X", "Y")
        );
        Assert.Equal("Cannot update: education 9 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task UpdateEducationAsync_InvalidId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.UpdateEducationAsync(0, "History", "Bachelor")
        );
    }
    
    [Fact]
    public async Task UpdateEducationAsync_EmptySpecialization_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.UpdateEducationAsync(1, "", "Bachelor")
        );
    }
    
    [Fact]
    public async Task UpdateEducationAsync_EmptyEducationLevel_ThrowsArgumentException()
    {
        // educationLevel empty or whitespace
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.UpdateEducationAsync(1, "History", "")
        );
    }
    
    [Fact]
    public async Task DeleteEducationAsync_Existing_Completes()
    {
        // Arrange
        _dimEducationRepositoryMock
            .Setup(r => r.DeleteEducationAsync(4))
            .Returns(Task.CompletedTask);

        // Act
        await _dimEducationService.DeleteEducationAsync(4);

        // Assert
        _dimEducationRepositoryMock.Verify(r => r.DeleteEducationAsync(4), Times.Once);
    }
    
    [Fact]
    public async Task DeleteEducationAsync_NotFound_ThrowsException()
    {
        // Arrange
        _dimEducationRepositoryMock
            .Setup(r => r.DeleteEducationAsync(6))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationService.DeleteEducationAsync(6)
        );
        Assert.Equal("Cannot delete: education 6 not found.", ex.Message);
    }
    
}
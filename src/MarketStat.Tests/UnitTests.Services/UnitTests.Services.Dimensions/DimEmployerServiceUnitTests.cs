using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimEmployerServiceUnitTests
{
    private readonly Mock<IDimEmployerRepository> _dimEmployerRepositoryMock;
    private readonly Mock<ILogger<DimEmployerService>> _loggerMock;
    private readonly DimEmployerService _dimEmployerService;

    public DimEmployerServiceUnitTests()
    {
        _dimEmployerRepositoryMock = new Mock<IDimEmployerRepository>();
        _loggerMock = new Mock<ILogger<DimEmployerService>>();
        _dimEmployerService = new DimEmployerService(_dimEmployerRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateEmployerAsync_ValidParameters_AssignsIdAndReturns()
    {
        _dimEmployerRepositoryMock.Setup(r => r.AddEmployerAsync(It.IsAny<DimEmployer>()))
             .Callback<DimEmployer>(e => e.EmployerId = 42)
             .Returns(Task.CompletedTask);

        var result = await _dimEmployerService.CreateEmployerAsync("Acme Corp", true);

        Assert.Equal(42, result.EmployerId);
        Assert.Equal("Acme Corp", result.EmployerName);
        Assert.True(result.IsPublic);
        _dimEmployerRepositoryMock.Verify(r => r.AddEmployerAsync(
            It.Is<DimEmployer>(e =>
                e.EmployerName == "Acme Corp" &&
                e.IsPublic)), Times.Once);
    }

    [Fact]
    public async Task CreateEmployerAsync_RepositoryThrowsConflict_ThrowsConflictException()
    {
        _dimEmployerRepositoryMock.Setup(r => r.AddEmployerAsync(It.IsAny<DimEmployer>()))
             .ThrowsAsync(new ConflictException("Duplicate"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _dimEmployerService.CreateEmployerAsync("Acme", false)
        );
    }

    [Fact]
    public async Task CreateEmployerAsync_InvalidName_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployerService.CreateEmployerAsync("", true)
        );
    }

    [Fact]
    public async Task GetEmployerByIdAsync_Found_ReturnsEntity()
    {
        var expected = new DimEmployer(7, "X", false);
        _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(7))
             .ReturnsAsync(expected);

        var actual = await _dimEmployerService.GetEmployerByIdAsync(7);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetEmployerByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(9))
             .ThrowsAsync(new NotFoundException("nope"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployerService.GetEmployerByIdAsync(9)
        );
    }

    [Fact]
    public async Task GetAllEmployersAsync_ReturnsAll()
    {
        var list = new[]
        {
            new DimEmployer(1, "A", true),
            new DimEmployer(2, "B", false)
        };
        _dimEmployerRepositoryMock.Setup(r => r.GetAllEmployersAsync())
             .ReturnsAsync(list);

        var result = await _dimEmployerService.GetAllEmployersAsync();

        Assert.Equal(list, result);
    }

    [Fact]
    public async Task UpdateEmployerAsync_ValidParameters_UpdatesAndReturns()
    {
        var existing = new DimEmployer(3, "Old", false);
        _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(3)).ReturnsAsync(existing);
        _dimEmployerRepositoryMock.Setup(r => r.UpdateEmployerAsync(It.IsAny<DimEmployer>())).Returns(Task.CompletedTask);

        var updated = await _dimEmployerService.UpdateEmployerAsync(3, "NewCo", true);

        Assert.Equal("NewCo", updated.EmployerName);
        Assert.True(updated.IsPublic);
        _dimEmployerRepositoryMock.Verify(r => r.UpdateEmployerAsync(
            It.Is<DimEmployer>(e =>
                e.EmployerId   == 3 &&
                e.EmployerName == "NewCo" &&
                e.IsPublic)), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployerAsync_InvalidId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployerService.UpdateEmployerAsync(0, "Any", false)
        );
    }

    [Fact]
    public async Task UpdateEmployerAsync_NotFound_ThrowsNotFoundException()
    {
        _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(5))
             .ThrowsAsync(new NotFoundException("missing"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployerService.UpdateEmployerAsync(5, "Name", true)
        );
    }

    [Fact]
    public async Task UpdateEmployerAsync_Conflict_ThrowsConflictException()
    {
        var existing = new DimEmployer(6, "Org", false);
        _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(6)).ReturnsAsync(existing);
        _dimEmployerRepositoryMock.Setup(r => r.UpdateEmployerAsync(It.IsAny<DimEmployer>()))
             .ThrowsAsync(new ConflictException("dupe"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _dimEmployerService.UpdateEmployerAsync(6, "Org2", true)
        );
    }

    [Fact]
    public async Task DeleteEmployerAsync_ValidId_Completes()
    {
        _dimEmployerRepositoryMock.Setup(r => r.DeleteEmployerAsync(8)).Returns(Task.CompletedTask);

        await _dimEmployerService.DeleteEmployerAsync(8);

        _dimEmployerRepositoryMock.Verify(r => r.DeleteEmployerAsync(8), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployerAsync_NotFound_ThrowsNotFoundException()
    {
        _dimEmployerRepositoryMock.Setup(r => r.DeleteEmployerAsync(9))
             .ThrowsAsync(new NotFoundException("gone"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployerService.DeleteEmployerAsync(9)
        );
    }
}
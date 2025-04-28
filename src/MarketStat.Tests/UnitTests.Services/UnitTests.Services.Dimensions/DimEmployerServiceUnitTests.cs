using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
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
    public async Task CreateEmployerAsync_EmptyRepo_CreatesWithId1()
    {
        _dimEmployerRepositoryMock
            .Setup(r => r.GetAllEmployersAsync())
            .ReturnsAsync(Array.Empty<DimEmployer>());

        var employer = await _dimEmployerService.CreateEmployerAsync("Acme", true);

        Assert.Equal(1, employer.EmployerId);
        Assert.Equal("Acme", employer.EmployerName);
        Assert.True(employer.IsPublic);
        _dimEmployerRepositoryMock.Verify(r => r.AddEmployerAsync(
            It.Is<DimEmployer>(e =>
                e.EmployerId == 1
                && e.EmployerName == "Acme"
                && e.IsPublic
            )), Times.Once);
    }
    
    [Fact]
    public async Task CreateEmployerAsync_NonEmptyRepo_IncrementsId()
    {
        var existing = new List<DimEmployer>
        {
            new DimEmployer(5, "Foo", false)
        };
        _dimEmployerRepositoryMock
            .Setup(r => r.GetAllEmployersAsync())
            .ReturnsAsync(existing);

        var employer = await _dimEmployerService.CreateEmployerAsync("NewCo", false);

        Assert.Equal(6, employer.EmployerId);
        _dimEmployerRepositoryMock.Verify(r => r.AddEmployerAsync(
            It.Is<DimEmployer>(e => e.EmployerId == 6)
        ), Times.Once);
    }
    
    [Fact]
    public async Task CreateEmployerAsync_InvalidName_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployerService.CreateEmployerAsync("", true)
        );
    }
    
    [Fact]
    public async Task CreateEmployerAsync_RepositoryThrows_WrapsException()
    {
        _dimEmployerRepositoryMock
            .Setup(r => r.GetAllEmployersAsync())
            .ReturnsAsync(Array.Empty<DimEmployer>());
        _dimEmployerRepositoryMock
            .Setup(r => r.AddEmployerAsync(It.IsAny<DimEmployer>()))
            .ThrowsAsync(new InvalidOperationException("db error"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployerService.CreateEmployerAsync("Name", false)
        );
        Assert.Contains("An employer with ID 1 already exists", ex.Message);
    }
    
    [Fact]
    public async Task GetEmployerByIdAsync_Found_ReturnsEmployer()
    {
        var e = new DimEmployer(2, "E2", true);
        _dimEmployerRepositoryMock
            .Setup(r => r.GetEmployerByIdAsync(2))
            .ReturnsAsync(e);

        var result = await _dimEmployerService.GetEmployerByIdAsync(2);

        Assert.Same(e, result);
    }
    
    [Fact]
    public async Task GetEmployerByIdAsync_NotFound_WrapsException()
    {
        _dimEmployerRepositoryMock
            .Setup(r => r.GetEmployerByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployerService.GetEmployerByIdAsync(99)
        );
        Assert.Contains("Employer with ID 99 was not found", ex.Message);
    }
    
    [Fact]
    public async Task GetAllEmployersAsync_ReturnsList()
    {
        var list = new List<DimEmployer>
        {
            new DimEmployer(1, "A" , false),
            new DimEmployer(2, "B", true)
        };
        _dimEmployerRepositoryMock
            .Setup(r => r.GetAllEmployersAsync())
            .ReturnsAsync(list);

        var result = await _dimEmployerService.GetAllEmployersAsync();

        Assert.Equal(2, result.Count());
        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task UpdateEmployerAsync_ValidParameters_UpdatesAndReturns()
    {
        var existing = new DimEmployer(3, "Old", false);
        _dimEmployerRepositoryMock
            .Setup(r => r.GetEmployerByIdAsync(3))
            .ReturnsAsync(existing);

        var updated = await _dimEmployerService.UpdateEmployerAsync(3, "New", true);

        Assert.Equal("New", updated.EmployerName);
        Assert.True(updated.IsPublic);
        _dimEmployerRepositoryMock.Verify(r => r.UpdateEmployerAsync(
            It.Is<DimEmployer>(e =>
                e.EmployerId == 3
                && e.EmployerName == "New"
                && e.IsPublic
            )), Times.Once);
    }
    
    [Fact]
    public async Task UpdateEmployerAsync_InvalidId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployerService.UpdateEmployerAsync(0, "N", false)
        );
    }
    
    [Fact]
    public async Task UpdateEmployerAsync_NotFound_WrapsException()
    {
        _dimEmployerRepositoryMock
            .Setup(r => r.GetEmployerByIdAsync(4))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployerService.UpdateEmployerAsync(4, "N", false)
        );
        Assert.Contains("Cannot update: employer 4 was not found", ex.Message);
    }
    
    [Fact]
    public async Task DeleteEmployerAsync_ValidId_CallsRepository()
    {
        await _dimEmployerService.DeleteEmployerAsync(7);
        _dimEmployerRepositoryMock.Verify(r => r.DeleteEmployerAsync(7), Times.Once);
    }
    
    [Fact]
    public async Task DeleteEmployerAsync_NotFound_WrapsException()
    {
        _dimEmployerRepositoryMock
            .Setup(r => r.DeleteEmployerAsync(8))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployerService.DeleteEmployerAsync(8)
        );
        Assert.Contains("Cannot delete: employer 8 not found", ex.Message);
    }
}
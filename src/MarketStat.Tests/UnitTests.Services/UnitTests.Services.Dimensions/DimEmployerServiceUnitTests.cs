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
    public async Task CreateEmployerAsync_EmptyRepo_AddsWithId1()
    {
        _dimEmployerRepositoryMock.Setup(r => r.GetAllEmployersAsync()).ReturnsAsync(Array.Empty<DimEmployer>());

        var result = await _dimEmployerService.CreateEmployerAsync("Amazon", "Logistics", false);

        Assert.Equal(1, result.EmployerId);
        Assert.Equal("Amazon", result.EmployerName);
        _dimEmployerRepositoryMock.Verify(r =>
            r.AddEmployerAsync(It.Is<DimEmployer>(e =>
                e.EmployerId   == 1 &&
                e.EmployerName == "Amazon" &&
                e.Industry     == "Logistics" &&
                e.IsPublic     == false
            )), Times.Once);
    }
}
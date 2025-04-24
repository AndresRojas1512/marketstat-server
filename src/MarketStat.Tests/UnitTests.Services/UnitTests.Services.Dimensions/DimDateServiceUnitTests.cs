using Microsoft.Extensions.Logging;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimDateService;
using Moq;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimDateServiceUnitTests
{
    private readonly Mock<IDimDateRepository> _dimDateRepositoryMock;
    private readonly Mock<ILogger<DimDateService>> _loggerMock;
    private readonly DimDateService _dimDateService;

    public DimDateServiceUnitTests()
    {
        _dimDateRepositoryMock = new Mock<IDimDateRepository>();
        _loggerMock = new Mock<ILogger<DimDateService>>();
        _dimDateService = new DimDateService(_dimDateRepositoryMock.Object, _loggerMock.Object);
    }
}
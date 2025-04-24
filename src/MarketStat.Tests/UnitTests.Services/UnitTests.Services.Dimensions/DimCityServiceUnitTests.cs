using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimCityService;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimCityServiceUnitTests
{
    private readonly Mock<IDimCityRepository> _dimCityRepositoryMock;
    private readonly Mock<ILogger<DimCityService>> _loggerMock;
    private readonly DimCityService _dimCityService;

    public DimCityServiceUnitTests()
    {
        _dimCityRepositoryMock = new Mock<IDimCityRepository>();
        _loggerMock = new Mock<ILogger<DimCityService>>();
        _dimCityService = new DimCityService(_dimCityRepositoryMock.Object, _loggerMock.Object);
    }
}
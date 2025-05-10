using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using Microsoft.Extensions.Logging;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimDateService;
using Moq;
using Xunit;

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
    
    [Fact]
    public async Task CreateDateAsync_ValidDate_ReturnsDimDateWithId()
    {
        var date = DateOnly.Parse("2025-04-24");

        _dimDateRepositoryMock
            .Setup(r => r.AddDateAsync(It.IsAny<DimDate>()))
            .Callback<DimDate>(d => d.DateId = 1)
            .Returns(Task.CompletedTask);

        var result = await _dimDateService.CreateDateAsync(date);

        Assert.Equal(1, result.DateId);
        Assert.Equal(date, result.FullDate);
        Assert.Equal(date.Year, result.Year);
        Assert.Equal(date.Month, result.Month);
        Assert.Equal((date.Month - 1) / 3 + 1, result.Quarter);

        _dimDateRepositoryMock.Verify(r => r.AddDateAsync(
            It.Is<DimDate>(d =>
                d.DateId   == 1 &&
                d.FullDate == date &&
                d.Year     == date.Year &&
                d.Month    == date.Month &&
                d.Quarter  == ((date.Month - 1) / 3 + 1)
            )), Times.Once);
    }
        
    [Fact]
    public async Task CreateDateAsync_InvalidDate_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimDateService.CreateDateAsync(default));
    }

    [Fact]
    public async Task CreateDateAsync_Duplicate_ThrowsConflictException()
    {
        var date = DateOnly.Parse("2025-04-24");
        var expectedMessage = $"A date for {date:yyyy-MM-dd} already exists.";

        _dimDateRepositoryMock
            .Setup(r => r.AddDateAsync(It.IsAny<DimDate>()))
            .ThrowsAsync(new ConflictException(expectedMessage));

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _dimDateService.CreateDateAsync(date));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public async Task GetDateByIdAsync_Existing_ReturnsDimDate()
    {
        var expected = new DimDate(7, DateOnly.Parse("2025-05-01"), 2025, 2, 5);
        _dimDateRepositoryMock
            .Setup(r => r.GetDateByIdAsync(7))
            .ReturnsAsync(expected);

        var actual = await _dimDateService.GetDateByIdAsync(7);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetDateByIdAsync_NotFound_ThrowsNotFoundException()
    {
        var id = 42;
        var expectedMessage = $"Date with ID {id} not found.";

        _dimDateRepositoryMock
            .Setup(r => r.GetDateByIdAsync(id))
            .ThrowsAsync(new NotFoundException(expectedMessage));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimDateService.GetDateByIdAsync(id));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public async Task GetAllDatesAsync_ReturnsList()
    {
        var list = new List<DimDate>
        {
            new DimDate(1, DateOnly.Parse("2025-01-01"), 2025, 1, 1),
            new DimDate(2, DateOnly.Parse("2025-04-01"), 2025, 2, 4)
        };
        _dimDateRepositoryMock
            .Setup(r => r.GetAllDatesAsync())
            .ReturnsAsync(list);

        var result = (await _dimDateService.GetAllDatesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(list, result);
    }

    [Fact]
    public async Task UpdateDateAsync_ValidParameters_ReturnsUpdatedDimDate()
    {
        var original = new DimDate(3, DateOnly.Parse("2025-02-01"), 2025, 1, 2);
        _dimDateRepositoryMock
            .Setup(r => r.GetDateByIdAsync(3))
            .ReturnsAsync(original);
        _dimDateRepositoryMock
            .Setup(r => r.UpdateDateAsync(It.IsAny<DimDate>()))
            .Returns(Task.CompletedTask);

        var newDate = DateOnly.Parse("2025-07-15");

        var updated = await _dimDateService.UpdateDateAsync(3, newDate);

        Assert.Equal(3, updated.DateId);
        Assert.Equal(newDate, updated.FullDate);
        Assert.Equal(newDate.Year,               updated.Year);
        Assert.Equal(newDate.Month,              updated.Month);
        Assert.Equal((newDate.Month - 1) / 3 + 1, updated.Quarter);

        _dimDateRepositoryMock.Verify(r => r.UpdateDateAsync(
            It.Is<DimDate>(d =>
                d.DateId   == 3 &&
                d.FullDate == newDate
            )), Times.Once);
    }

    [Fact]
    public async Task UpdateDateAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimDateService.UpdateDateAsync(0, DateOnly.Parse("2025-08-01")));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimDateService.UpdateDateAsync(1, default));
    }

    [Fact]
    public async Task UpdateDateAsync_NotFound_ThrowsNotFoundException()
    {
        var id = 9;
        var expectedMessage = $"Date with ID {id} not found.";

        _dimDateRepositoryMock
            .Setup(r => r.GetDateByIdAsync(id))
            .ThrowsAsync(new NotFoundException(expectedMessage));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimDateService.UpdateDateAsync(id, DateOnly.Parse("2025-08-01")));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public async Task DeleteDateAsync_NotFound_ThrowsNotFoundException()
    {
        var id = 8;
        var expectedMessage = $"Date with ID {id} not found.";

        _dimDateRepositoryMock
            .Setup(r => r.DeleteDateAsync(id))
            .ThrowsAsync(new NotFoundException(expectedMessage));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimDateService.DeleteDateAsync(id));

        Assert.Equal(expectedMessage, ex.Message);
    }
}
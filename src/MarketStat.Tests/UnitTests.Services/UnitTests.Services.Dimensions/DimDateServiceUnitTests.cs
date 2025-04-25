using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
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
    public async Task CreateDateAsync_ValidDate_ReturnsDimDate()
    {
        var date = DateOnly.Parse("2025-04-24");
        _dimDateRepositoryMock
            .Setup(r => r.GetAllDatesAsync())
            .ReturnsAsync(new List<DimDate>());
        _dimDateRepositoryMock
            .Setup(r => r.AddDateAsync(It.IsAny<DimDate>()))
            .Returns(Task.CompletedTask);

        var result = await _dimDateService.CreateDateAsync(date);

        Assert.Equal(1, result.DateId);
        Assert.Equal(date, result.FullDate);
        Assert.Equal(date.Year, result.Year);
        Assert.Equal(date.Month, result.Month);
        Assert.Equal((date.Month - 1) / 3 + 1, result.Quarter);

        _dimDateRepositoryMock.Verify(r => r.AddDateAsync(
            It.Is<DimDate>(d =>
                d.DateId == 1 &&
                d.FullDate == date &&
                d.Year == date.Year &&
                d.Month == date.Month &&
                d.Quarter == ((date.Month - 1) / 3 + 1)
            )), Times.Once);
    }
    
    [Fact]
    public async Task CreateDateAsync_Duplicate_ThrowsException()
    {
        var date = DateOnly.Parse("2025-04-24");
        _dimDateRepositoryMock
            .Setup(r => r.GetAllDatesAsync())
            .ReturnsAsync(new List<DimDate>());
        _dimDateRepositoryMock
            .Setup(r => r.AddDateAsync(It.IsAny<DimDate>()))
            .ThrowsAsync(new InvalidOperationException("exists"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimDateService.CreateDateAsync(date));

        Assert.Equal($"A dim_date row for {date} already exists.", ex.Message);
    }
    
    [Fact]
    public async Task CreateDateAsync_InvalidDate_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimDateService.CreateDateAsync(default));
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
    public async Task GetDateByIdAsync_NotFound_ThrowsException()
    {
        _dimDateRepositoryMock
            .Setup(r => r.GetDateByIdAsync(42))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimDateService.GetDateByIdAsync(42));

        Assert.Equal("Date with ID 42 was not found.", ex.Message);
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
        Assert.Equal(newDate.Year, updated.Year);
        Assert.Equal(newDate.Month, updated.Month);
        Assert.Equal((newDate.Month - 1) / 3 + 1, updated.Quarter);

        _dimDateRepositoryMock.Verify(r => r.UpdateDateAsync(
            It.Is<DimDate>(d =>
                d.DateId == 3 &&
                d.FullDate == newDate
            )), Times.Once);
    }
    
    [Fact]
    public async Task UpdateDateAsync_NotFound_ThrowsException()
    {
        _dimDateRepositoryMock
            .Setup(r => r.GetDateByIdAsync(9))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimDateService.UpdateDateAsync(9, DateOnly.Parse("2025-08-01")));

        Assert.Equal("Cannot update: date 9 not found.", ex.Message);
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
    public async Task DeleteDateAsync_Existing_Completes()
    {
        _dimDateRepositoryMock
            .Setup(r => r.DeleteDateAsync(5))
            .Returns(Task.CompletedTask);

        await _dimDateService.DeleteDateAsync(5);

        _dimDateRepositoryMock.Verify(r => r.DeleteDateAsync(5), Times.Once);
    }
    
    [Fact]
    public async Task DeleteDateAsync_NotFound_ThrowsException()
    {
        _dimDateRepositoryMock
            .Setup(r => r.DeleteDateAsync(8))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimDateService.DeleteDateAsync(8));

        Assert.Equal("Cannot delete: date 8 not found.", ex.Message);
    }
}
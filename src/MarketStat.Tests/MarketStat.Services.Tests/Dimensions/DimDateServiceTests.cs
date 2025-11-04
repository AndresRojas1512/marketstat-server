using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimDateService;
using MarketStat.Tests.TestData.ObjectMothers.Dimensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketStat.Services.Tests.Dimensions;

public class DimDateServiceTests
{
    private readonly Mock<IDimDateRepository> _mockRepository;
    private readonly Mock<ILogger<DimDateService>> _mockLogger;

    private readonly DimDateService _sut;

    public DimDateServiceTests()
    {
        _mockRepository = new Mock<IDimDateRepository>();
        _mockLogger = new Mock<ILogger<DimDateService>>();
        _sut = new DimDateService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateDateAsync_ShouldCallAddDateAsync_WhenDataIsValid()
    {
        var newDate = DimDateObjectMother.ANewDate();
        _mockRepository.Setup(repo => repo.AddDateAsync(It.IsAny<DimDate>()))
            .Callback<DimDate>(date => date.DateId = 1)
            .Returns(Task.CompletedTask);
        var result = await _sut.CreateDateAsync(newDate.FullDate);
        _mockRepository.Verify(repo => repo.AddDateAsync(
            It.Is<DimDate>(d => d.FullDate == newDate.FullDate)
        ), Times.Once);
        
        result.Should().NotBeNull();
        result.DateId.Should().Be(1);
        result.FullDate.Should().Be(newDate.FullDate);
        result.Year.Should().Be(newDate.Year);
        result.Quarter.Should().Be(newDate.Quarter);
        result.Month.Should().Be(newDate.Month);
    }

    [Fact]
    public async Task CreateDateAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var newDate = DimDateObjectMother.ANewDate();
        _mockRepository.Setup(repo => repo.AddDateAsync(It.IsAny<DimDate>()))
            .ThrowsAsync(new ConflictException("Date already exists."));
        Func<Task> act = async () => await _sut.CreateDateAsync(newDate.FullDate);
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task GetDateByIdAsync_ShouldReturnDate_WhenDateExists()
    {
        var expectedDate = DimDateObjectMother.AnExistingDate();
        _mockRepository.Setup(repo => repo.GetDateByIdAsync(expectedDate.DateId))
                 .ReturnsAsync(expectedDate);
        var result = await _sut.GetDateByIdAsync(expectedDate.DateId);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDate);
    }
    
    [Fact]
    public async Task GetDateByIdAsync_ShouldThrowNotFoundException_WhenDateDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetDateByIdAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.GetDateByIdAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetAllDatesAsync_ShouldReturnAllDates_WhenDatesExist()
    {
        var expectedList = DimDateObjectMother.SomeDates();
        _mockRepository.Setup(repo => repo.GetAllDatesAsync()).ReturnsAsync(expectedList);
        var result = await _sut.GetAllDatesAsync();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedList);
    }

    [Fact]
    public async Task GetAllDatesAsync_ShouldReturnEmptyList_WhenNoDatesExist()
    {
        _mockRepository.Setup(repo => repo.GetAllDatesAsync()).ReturnsAsync(new List<DimDate>());
        var result = await _sut.GetAllDatesAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateDateAsync_ShouldCallUpdateDateAsync_WhenDataIsValid()
    {
        var existingDate = DimDateObjectMother.AnExistingDate();
        var newFullDate = new DateOnly(2024, 7, 1);

        _mockRepository.Setup(repo => repo.GetDateByIdAsync(existingDate.DateId))
                 .ReturnsAsync(existingDate);
        _mockRepository.Setup(repo => repo.UpdateDateAsync(It.IsAny<DimDate>()))
                 .Returns(Task.CompletedTask);
        var result = await _sut.UpdateDateAsync(existingDate.DateId, newFullDate);
        _mockRepository.Verify(repo => repo.UpdateDateAsync(
            It.Is<DimDate>(d => 
                d.DateId == existingDate.DateId &&
                d.FullDate == newFullDate &&
                d.Quarter == 3
            )
        ), Times.Once);
        result.FullDate.Should().Be(newFullDate);
        result.Quarter.Should().Be(3);
        result.Year.Should().Be(2024);
    }

    [Fact]
    public async Task UpdateDateAsync_ShouldThrowNotFoundException_WhenDateDoesNotExist()
    {
        var nonExistentId = 999;
        var newFullDate = new DateOnly(2024, 7, 1);
        _mockRepository.Setup(repo => repo.GetDateByIdAsync(nonExistentId))
                 .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.UpdateDateAsync(nonExistentId, newFullDate);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateDateAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var existingDate = DimDateObjectMother.AnExistingDate();
        var newFullDate = new DateOnly(2024, 7, 1);
        _mockRepository.Setup(repo => repo.GetDateByIdAsync(existingDate.DateId))
                 .ReturnsAsync(existingDate);
        _mockRepository.Setup(repo => repo.UpdateDateAsync(It.IsAny<DimDate>()))
                 .ThrowsAsync(new ConflictException("Conflict on update."));
        Func<Task> act = async () => await _sut.UpdateDateAsync(existingDate.DateId, newFullDate);
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task DeleteDateAsync_ShouldCallDeleteDateAsync_WhenDateExists()
    {
        var existingId = 1;
        _mockRepository.Setup(repo => repo.DeleteDateAsync(existingId))
                 .Returns(Task.CompletedTask);
        await _sut.DeleteDateAsync(existingId);
        _mockRepository.Verify(repo => repo.DeleteDateAsync(existingId), Times.Once);
    }

    [Fact]
    public async Task DeleteDateAsync_ShouldThrowNotFoundException_WhenDateDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.DeleteDateAsync(nonExistentId))
                 .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.DeleteDateAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
using AutoMapper;
using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using MarketStat.Services.Facts.FactSalaryService;
using MarketStat.Tests.TestData.ObjectMothers.Facts;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketStat.Services.Tests.Facts;

public class FactSalaryServiceTests
{
    private readonly Mock<IFactSalaryRepository> _mockFactSalaryRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<FactSalaryService>> _mockLogger;
    private readonly Mock<IDimLocationRepository> _mockLocationRepository;
    private readonly Mock<IDimJobRepository> _mockJobRepository;
    private readonly Mock<IDimIndustryFieldService> _mockIndustryFieldService;
    
    private readonly FactSalaryService _sut;
    
    public FactSalaryServiceTests()
    {
        _mockFactSalaryRepository = new Mock<IFactSalaryRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<FactSalaryService>>();
        _mockLocationRepository = new Mock<IDimLocationRepository>();
        _mockJobRepository = new Mock<IDimJobRepository>();
        _mockIndustryFieldService = new Mock<IDimIndustryFieldService>();

        _sut = new FactSalaryService(
            _mockFactSalaryRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockLocationRepository.Object,
            _mockJobRepository.Object,
            _mockIndustryFieldService.Object
        );
    }
    
    [Fact]
    public async Task CreateFactSalaryAsync_ShouldCallAddFactSalaryAsync_WhenDataIsValid()
    {
        var newSalary = FactSalaryObjectMother.ANewSalary();
        _mockFactSalaryRepository.Setup(repo => repo.AddFactSalaryAsync(It.IsAny<FactSalary>()))
            .Callback<FactSalary>(salary => salary.SalaryFactId = 1L)
            .Returns(Task.CompletedTask);
        var result = await _sut.CreateFactSalaryAsync(
            newSalary.DateId, newSalary.LocationId, newSalary.EmployerId,
            newSalary.JobId, newSalary.EmployeeId, newSalary.SalaryAmount
        );
        _mockFactSalaryRepository.Verify(repo => repo.AddFactSalaryAsync(It.IsAny<FactSalary>()), Times.Once);
        result.Should().NotBeNull();
        result.SalaryFactId.Should().Be(1L);
    }
    
    [Fact]
    public async Task CreateFactSalaryAsync_ShouldThrowNotFoundException_WhenRepoThrowsForeignKeyViolation()
    {
        var newSalary = FactSalaryObjectMother.ANewSalary();
        _mockFactSalaryRepository.Setup(repo => repo.AddFactSalaryAsync(It.IsAny<FactSalary>()))
            .ThrowsAsync(new NotFoundException("FK violation."));
        Func<Task> act = async () => await _sut.CreateFactSalaryAsync(
            newSalary.DateId, newSalary.LocationId, newSalary.EmployerId,
            newSalary.JobId, newSalary.EmployeeId, newSalary.SalaryAmount
        );
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetFactSalaryByIdAsync_ShouldReturnSalary_WhenSalaryExists()
    {
        var expectedSalary = FactSalaryObjectMother.AnExistingSalary();
        _mockFactSalaryRepository.Setup(repo => repo.GetFactSalaryByIdAsync(expectedSalary.SalaryFactId))
            .ReturnsAsync(expectedSalary);
        var result = await _sut.GetFactSalaryByIdAsync(expectedSalary.SalaryFactId);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedSalary);
    }
    
    [Fact]
    public async Task GetFactSalaryByIdAsync_ShouldThrowNotFoundException_WhenSalaryDoesNotExist()
    {
        var nonExistentId = 999L;
        _mockFactSalaryRepository.Setup(repo => repo.GetFactSalaryByIdAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.GetFactSalaryByIdAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task UpdateFactSalaryAsync_ShouldCallUpdateFactSalaryAsync_WhenDataIsValid()
    {
        var existingSalary = FactSalaryObjectMother.AnExistingSalary();
        var updatedAmount = 99999m;
        _mockFactSalaryRepository.Setup(repo => repo.GetFactSalaryByIdAsync(existingSalary.SalaryFactId))
            .ReturnsAsync(existingSalary);
        _mockFactSalaryRepository.Setup(repo => repo.UpdateFactSalaryAsync(It.IsAny<FactSalary>()))
            .Returns(Task.CompletedTask);
        var result = await _sut.UpdateFactSalaryAsync(
            existingSalary.SalaryFactId, existingSalary.DateId, existingSalary.LocationId,
            existingSalary.EmployerId, existingSalary.JobId, existingSalary.EmployeeId,
            updatedAmount
        );
        _mockFactSalaryRepository.Verify(repo => repo.UpdateFactSalaryAsync(
            It.Is<FactSalary>(s => s.SalaryFactId == existingSalary.SalaryFactId && s.SalaryAmount == updatedAmount)
        ), Times.Once);
        result.SalaryAmount.Should().Be(updatedAmount);
    }

    [Fact]
    public async Task UpdateFactSalaryAsync_ShouldThrowNotFoundException_WhenSalaryDoesNotExist()
    {
        var nonExistentId = 999L;
        _mockFactSalaryRepository.Setup(repo => repo.GetFactSalaryByIdAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.UpdateFactSalaryAsync(
            nonExistentId, 1, 1, 1, 1, 1, 100
        );
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task DeleteFactSalaryAsync_ShouldCallDeleteFactSalaryByIdAsync_WhenSalaryExists()
    {
        var existingId = 1L;
        _mockFactSalaryRepository.Setup(repo => repo.DeleteFactSalaryByIdAsync(existingId))
            .Returns(Task.CompletedTask);
        await _sut.DeleteFactSalaryAsync(existingId);
        _mockFactSalaryRepository.Verify(repo => repo.DeleteFactSalaryByIdAsync(existingId), Times.Once);
    }

    [Fact]
    public async Task DeleteFactSalaryAsync_ShouldThrowNotFoundException_WhenSalaryDoesNotExist()
    {
        var nonExistentId = 999L;
        _mockFactSalaryRepository.Setup(repo => repo.DeleteFactSalaryByIdAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.DeleteFactSalaryAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetFactSalariesByFilterAsync_ShouldResolveFiltersAndCallRepository_WhenFiltersMatch()
    {
        var userFilter = new SalaryFilterDto { CityName = "Moscow" };
        var resolvedLocationIds = new List<int> { 1, 2 };
        var expectedSalaries = FactSalaryObjectMother.SomeSalaries();
        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "Moscow"))
            .ReturnsAsync(resolvedLocationIds);
        _mockFactSalaryRepository.Setup(repo => repo.GetFactSalariesByFilterAsync(
                It.Is<ResolvedSalaryFilterDto>(dto => dto.LocationIds == resolvedLocationIds)
            ))
            .ReturnsAsync(expectedSalaries);
        var result = await _sut.GetFactSalariesByFilterAsync(userFilter);
        result.Should().BeEquivalentTo(expectedSalaries);
        _mockFactSalaryRepository.Verify(repo => repo.GetFactSalariesByFilterAsync(It.IsAny<ResolvedSalaryFilterDto>()), Times.Once);
    }
    
    [Fact]
    public async Task GetFactSalariesByFilterAsync_ShouldReturnEmpty_WhenFiltersDoNotResolve()
    {
        var userFilter = new SalaryFilterDto { CityName = "NonExistentCity" };
        var resolvedLocationIds = new List<int>();
        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "NonExistentCity"))
            .ReturnsAsync(resolvedLocationIds);
        var result = await _sut.GetFactSalariesByFilterAsync(userFilter);
        result.Should().BeEmpty();
        _mockFactSalaryRepository.Verify(repo => repo.GetFactSalariesByFilterAsync(It.IsAny<ResolvedSalaryFilterDto>()), Times.Never);
    }
}
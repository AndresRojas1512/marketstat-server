using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Responses;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Services.Facts.FactSalaryService;
using MarketStat.Tests.TestData.ObjectMothers.Facts;
using Microsoft.Extensions.Logging;
using MarketStat.Common.Enums;
using MarketStat.Tests.TestData.Builders.Dimensions;
using Moq;

namespace MarketStat.Services.Tests.Facts;

public class FactSalaryServiceTests
{
    private readonly Mock<IFactSalaryRepository> _mockFactSalaryRepository;
    private readonly Mock<ILogger<FactSalaryService>> _mockLogger;
    private readonly Mock<IDimLocationRepository> _mockLocationRepository;
    private readonly Mock<IDimJobRepository> _mockJobRepository;
    private readonly Mock<IDimIndustryFieldRepository> _mockIndustryFieldRepository;
    
    private readonly FactSalaryService _sut;
    
    public FactSalaryServiceTests()
    {
        _mockFactSalaryRepository = new Mock<IFactSalaryRepository>();
        _mockLogger = new Mock<ILogger<FactSalaryService>>();
        _mockLocationRepository = new Mock<IDimLocationRepository>();
        _mockJobRepository = new Mock<IDimJobRepository>();
        _mockIndustryFieldRepository = new Mock<IDimIndustryFieldRepository>();

        _sut = new FactSalaryService(
            _mockFactSalaryRepository.Object,
            _mockLogger.Object,
            _mockLocationRepository.Object,
            _mockJobRepository.Object,
            _mockIndustryFieldRepository.Object
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
        var userFilter = new AnalysisFilterRequest { CityName = "Moscow" };
        var resolvedLocationIds = new List<int> { 1, 2 };
        var expectedSalaries = FactSalaryObjectMother.SomeSalaries();
        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "Moscow"))
            .ReturnsAsync(resolvedLocationIds);
        _mockFactSalaryRepository.Setup(repo => repo.GetFactSalariesByFilterAsync(
                It.Is<ResolvedSalaryFilter>(dto => dto.LocationIds == resolvedLocationIds)
            ))
            .ReturnsAsync(expectedSalaries);
        var result = await _sut.GetFactSalariesByFilterAsync(userFilter);
        result.Should().BeEquivalentTo(expectedSalaries);
        _mockFactSalaryRepository.Verify(repo => repo.GetFactSalariesByFilterAsync(It.IsAny<ResolvedSalaryFilter>()), Times.Once);
    }
    
    [Fact]
    public async Task GetFactSalariesByFilterAsync_ShouldReturnEmpty_WhenFiltersDoNotResolve()
    {
        var userFilter = new AnalysisFilterRequest { CityName = "NonExistentCity" };
        var resolvedLocationIds = new List<int>();
        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "NonExistentCity"))
            .ReturnsAsync(resolvedLocationIds);
        var result = await _sut.GetFactSalariesByFilterAsync(userFilter);
        result.Should().BeEmpty();
        _mockFactSalaryRepository.Verify(repo => repo.GetFactSalariesByFilterAsync(It.IsAny<ResolvedSalaryFilter>()), Times.Never);
    }
    
    [Fact]
    public async Task GetFactSalariesByFilterAsync_ShouldResolveIndustryName_WhenIndustryNameIsValid()
    {
        var userFilter = new AnalysisFilterRequest { IndustryFieldName = "IT" };
        var mockIndustry = new DimIndustryFieldBuilder().WithId(1).WithIndustryFieldName("IT").Build();
        var resolvedJobIds = new List<int> { 10, 11 };
        _mockIndustryFieldRepository.Setup(r => r.GetIndustryFieldByNameAsync("IT"))
            .ReturnsAsync(mockIndustry);
        _mockJobRepository.Setup(r => r.GetJobIdsByFilterAsync(null, null, 1))
            .ReturnsAsync(resolvedJobIds);
        _mockFactSalaryRepository.Setup(r => r.GetFactSalariesByFilterAsync(
            It.Is<ResolvedSalaryFilter>(f => f.JobIds == resolvedJobIds)))
            .ReturnsAsync(new List<FactSalary>());
        await _sut.GetFactSalariesByFilterAsync(userFilter);
        _mockIndustryFieldRepository.Verify(r => r.GetIndustryFieldByNameAsync("IT"), Times.Once);
        _mockJobRepository.Verify(r => r.GetJobIdsByFilterAsync(null, null, 1), Times.Once);
        _mockFactSalaryRepository.Verify(r => r.GetFactSalariesByFilterAsync(It.IsAny<ResolvedSalaryFilter>()), Times.Once);
    }

    [Fact]
    public async Task GetFactSalariesByFilterAsync_ShouldThrowArgumentException_WhenIndustryNameIsInvalid()
    {
        var userFilter = new AnalysisFilterRequest { IndustryFieldName = "NonExistentIndustry" };
        _mockIndustryFieldRepository.Setup(r => r.GetIndustryFieldByNameAsync("NonExistentIndustry"))
            .ReturnsAsync((DimIndustryField?)null);
        Func<Task> act = async () => await _sut.GetFactSalariesByFilterAsync(userFilter);
        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("Invalid IndustryFieldName provided: NonExistentIndustry");
        _mockJobRepository.Verify(r => r.GetJobIdsByFilterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        _mockFactSalaryRepository.Verify(r => r.GetFactSalariesByFilterAsync(It.IsAny<ResolvedSalaryFilter>()), Times.Never);
    }
    
    [Fact]
    public async Task GetSalaryDistributionAsync_ShouldReturnDistribution_WhenFiltersResolve()
    {
        var request = new AnalysisFilterRequest { CityName = "Moscow" };
        var resolvedLocationIds = new List<int> { 1 };
        var expectedDistribution = new List<SalaryDistributionBucket>();

        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "Moscow"))
            .ReturnsAsync(resolvedLocationIds);
        
        _mockFactSalaryRepository.Setup(repo => repo.GetSalaryDistributionAsync(It.Is<ResolvedSalaryFilter>(f => f.LocationIds == resolvedLocationIds)))
            .ReturnsAsync(expectedDistribution);
        var result = await _sut.GetSalaryDistributionAsync(request);
        result.Should().BeEquivalentTo(expectedDistribution); 
        _mockFactSalaryRepository.Verify(repo => repo.GetSalaryDistributionAsync(It.IsAny<ResolvedSalaryFilter>()), Times.Once);
    }

    [Fact]
    public async Task GetSalaryDistributionAsync_ShouldReturnEmpty_WhenFiltersDoNotResolve()
    {
        var request = new AnalysisFilterRequest { CityName = "NonExistent" };
        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "NonExistent"))
            .ReturnsAsync(new List<int>());
        var result = await _sut.GetSalaryDistributionAsync(request);
        result.Should().BeEmpty();
        _mockFactSalaryRepository.Verify(repo => repo.GetSalaryDistributionAsync(It.IsAny<ResolvedSalaryFilter>()), Times.Never);
    }
    
    [Fact]
    public async Task GetSalarySummaryAsync_ShouldReturnSummary_WhenFiltersResolve()
    {
        var request = new SalarySummaryRequest { CityName = "Moscow", TargetPercentile = 90 };
        var resolvedLocationIds = new List<int> { 1 };
        var expectedSummary = new SalarySummary { TotalCount = 10 };
        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "Moscow"))
            .ReturnsAsync(resolvedLocationIds);
        _mockFactSalaryRepository.Setup(repo => repo.GetSalarySummaryAsync(It.Is<ResolvedSalaryFilter>(f => f.LocationIds == resolvedLocationIds), 90))
            .ReturnsAsync(expectedSummary);
        var result = await _sut.GetSalarySummaryAsync(request);
        result.Should().Be(expectedSummary);
        _mockFactSalaryRepository.Verify(repo => repo.GetSalarySummaryAsync(It.IsAny<ResolvedSalaryFilter>(), 90), Times.Once);
    }

    [Fact]
    public async Task GetSalarySummaryAsync_ShouldReturnNull_WhenFiltersDoNotResolve()
    {
        var request = new SalarySummaryRequest { CityName = "NonExistent" };
        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "NonExistent"))
            .ReturnsAsync(new List<int>());
        var result = await _sut.GetSalarySummaryAsync(request);
        result.Should().BeNull();
        _mockFactSalaryRepository.Verify(repo => repo.GetSalarySummaryAsync(It.IsAny<ResolvedSalaryFilter>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetSalarySummaryAsync_ShouldThrowArgumentException_WhenPercentileIsInvalid()
    {
        var request = new SalarySummaryRequest { TargetPercentile = -10 };
        Func<Task> act = async () => await _sut.GetSalarySummaryAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("TargetPercentile");
    }
    
    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldReturnSeries_WhenFiltersResolve()
    {
        var request = new TimeSeriesRequest { CityName = "Moscow", Granularity = TimeGranularity.Month, Periods = 6 };
        var resolvedLocationIds = new List<int> { 1 };
        var expectedSeries = new List<SalaryTimeSeriesPoint>();
        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "Moscow"))
            .ReturnsAsync(resolvedLocationIds);
        _mockFactSalaryRepository.Setup(repo => repo.GetSalaryTimeSeriesAsync(
                It.Is<ResolvedSalaryFilter>(f => f.LocationIds == resolvedLocationIds), TimeGranularity.Month, 6))
            .ReturnsAsync(expectedSeries);
        var result = await _sut.GetSalaryTimeSeriesAsync(request);
        result.Should().BeEquivalentTo(expectedSeries);
        _mockFactSalaryRepository.Verify(repo => repo.GetSalaryTimeSeriesAsync(It.IsAny<ResolvedSalaryFilter>(), TimeGranularity.Month, 6), Times.Once);
    }
    
    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldReturnEmpty_WhenFiltersDoNotResolve()
    {
        var request = new TimeSeriesRequest { CityName = "NonExistent", Periods = 1 }; 
        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "NonExistent"))
            .ReturnsAsync(new List<int>());
        var result = await _sut.GetSalaryTimeSeriesAsync(request);
        result.Should().BeEmpty();
        _mockFactSalaryRepository.Verify(repo => repo.GetSalaryTimeSeriesAsync(It.IsAny<ResolvedSalaryFilter>(), It.IsAny<TimeGranularity>(), It.IsAny<int>()), Times.Never);
    }
    
    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldThrowArgumentException_WhenPeriodsIsInvalid()
    {
        var request = new TimeSeriesRequest { Periods = 0 };
        Func<Task> act = async () => await _sut.GetSalaryTimeSeriesAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("Periods");
    }
    
    [Fact]
    public async Task GetPublicRolesAsync_ShouldReturnRoles_WhenFiltersResolve()
    {
        var request = new PublicRolesRequest { CityName = "Moscow", MinRecordCount = 10 };
        var resolvedLocationIds = new List<int> { 1 };
        var expectedRoles = new List<PublicRoleByLocationIndustry>();
        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "Moscow"))
            .ReturnsAsync(resolvedLocationIds);
        _mockFactSalaryRepository.Setup(repo => repo.GetPublicRolesAsync(
                It.Is<ResolvedSalaryFilter>(f => f.LocationIds == resolvedLocationIds), 10))
            .ReturnsAsync(expectedRoles);
        var result = await _sut.GetPublicRolesAsync(request);
        result.Should().BeEquivalentTo(expectedRoles);
        _mockFactSalaryRepository.Verify(repo => repo.GetPublicRolesAsync(It.IsAny<ResolvedSalaryFilter>(), 10), Times.Once);
    }
    
    [Fact]
    public async Task GetPublicRolesAsync_ShouldReturnEmpty_WhenFiltersDoNotResolve()
    {
        var request = new PublicRolesRequest { CityName = "NonExistent" };
        _mockLocationRepository.Setup(repo => repo.GetLocationIdsByFilterAsync(null, null, "NonExistent"))
            .ReturnsAsync(new List<int>());
        var result = await _sut.GetPublicRolesAsync(request);
        result.Should().BeEmpty();
        _mockFactSalaryRepository.Verify(repo => repo.GetPublicRolesAsync(It.IsAny<ResolvedSalaryFilter>(), It.IsAny<int>()), Times.Never);
    }
    
    [Fact]
    public async Task GetPublicRolesAsync_ShouldThrowArgumentException_WhenMinCountIsInvalid()
    {
        var request = new PublicRolesRequest { MinRecordCount = -1 };
        Func<Task> act = async () => await _sut.GetPublicRolesAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("MinRecordCount");
    }
}
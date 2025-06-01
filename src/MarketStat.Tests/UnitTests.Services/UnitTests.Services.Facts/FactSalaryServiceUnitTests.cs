using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Facts;

public class FactSalaryServiceUnitTests
{
    private readonly Mock<IFactSalaryRepository> _factSalaryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<FactSalaryService>> _loggerMock;
    private readonly Mock<MarketStatDbContext> _dbContextMock;
    
    private readonly FactSalaryService _factSalaryService;

    public FactSalaryServiceUnitTests()
    {
        _factSalaryRepositoryMock = new Mock<IFactSalaryRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<FactSalaryService>>();
        
        var mockDbContextOptions = new DbContextOptions<MarketStatDbContext>();
        _dbContextMock = new Mock<MarketStatDbContext>(mockDbContextOptions);

        _factSalaryService = new FactSalaryService(
            _factSalaryRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _dbContextMock.Object
        );
    }
    
    [Fact]
    public async Task CreateFactSalaryAsync_ValidParameters_ReturnsNewFact()
    {
        _factSalaryRepositoryMock
            .Setup(r => r.AddFactSalaryAsync(It.IsAny<FactSalary>()))
            .Callback<FactSalary>(f => f.SalaryFactId = 1)
            .Returns(Task.CompletedTask);

        var fact = await _factSalaryService.CreateFactSalaryAsync(
            dateId:       1,
            cityId:       2,
            employerId:   3,
            jobRoleId:    4,
            employeeId:   5,
            salaryAmount: 1000m,
            bonusAmount:  100m
        );

        Assert.NotNull(fact);
        Assert.Equal(1,    fact.SalaryFactId);
        Assert.Equal(1000m, fact.SalaryAmount);
        Assert.Equal(100m,  fact.BonusAmount);
        _factSalaryRepositoryMock.Verify(r => r.AddFactSalaryAsync(
            It.Is<FactSalary>(f =>
                f.DateId       == 1 &&
                f.CityId       == 2 &&
                f.EmployerId   == 3 &&
                f.JobRoleId    == 4 &&
                f.EmployeeId   == 5 &&
                f.SalaryAmount == 1000m &&
                f.BonusAmount  == 100m
            )), Times.Once);
    }

    [Fact]
    public async Task CreateFactSalaryAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factSalaryService.CreateFactSalaryAsync(
                dateId:       0,
                cityId:       1,
                employerId:   1,
                jobRoleId:    1,
                employeeId:   1,
                salaryAmount: 1000m,
                bonusAmount:  100m));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factSalaryService.CreateFactSalaryAsync(
                dateId:       1,
                cityId:       1,
                employerId:   1,
                jobRoleId:    1,
                employeeId:   1,
                salaryAmount: -5m,
                bonusAmount:  0m));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factSalaryService.CreateFactSalaryAsync(
                dateId:       1,
                cityId:       1,
                employerId:   1,
                jobRoleId:    1,
                employeeId:   1,
                salaryAmount: 100m,
                bonusAmount:  -1m));
    }

    [Fact]
    public async Task CreateFactSalaryAsync_FkMissing_ThrowsNotFoundException()
    {
        _factSalaryRepositoryMock
            .Setup(r => r.AddFactSalaryAsync(It.IsAny<FactSalary>()))
            .ThrowsAsync(new NotFoundException("Foreign key not found"));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _factSalaryService.CreateFactSalaryAsync(
                dateId:       1,
                cityId:       2,
                employerId:   3,
                jobRoleId:    4,
                employeeId:   5,
                salaryAmount: 1000m,
                bonusAmount:  100m));

        Assert.Equal("Foreign key not found", ex.Message);
    }

    [Fact]
    public async Task GetFactSalaryByIdAsync_ExistingId_ReturnsFact()
    {
        var expected = new FactSalary(7,1,2,3,4,5,2000m,200m);
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalaryByIdAsync(7))
            .ReturnsAsync(expected);

        var actual = await _factSalaryService.GetFactSalaryByIdAsync(7);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetFactSalaryByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalaryByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("FactSalary 99 was not found."));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _factSalaryService.GetFactSalaryByIdAsync(99));

        Assert.Equal("FactSalary 99 was not found.", ex.Message);
    }

    [Fact]
    public async Task GetAllFactSalariesAsync_ReturnsList()
    {
        var list = new List<FactSalary>
        {
            new FactSalary(1,1,1,1,1,1,100m,0m),
            new FactSalary(2,2,2,2,2,2,200m,20m)
        };
        _factSalaryRepositoryMock
            .Setup(r => r.GetAllFactSalariesAsync())
            .ReturnsAsync(list);

        var actual = (await _factSalaryService.GetAllFactSalariesAsync()).ToList();

        Assert.Equal(2, actual.Count);
        Assert.Equal(list, actual);
    }

    [Fact]
    public async Task GetFactSalariesByFilterAsync_FilteredCorrectly()
    {
        var all = new[]
        {
            new FactSalary(1,1,10,1,1,1,100m,0m),
            new FactSalary(2,2,10,1,1,1,200m,0m),
            new FactSalary(3,1,20,1,1,1,300m,0m)
        };
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalariesByFilterAsync(It.IsAny<SalaryFilterDto>()))
            .ReturnsAsync((SalaryFilterDto f) =>
                all.Where(x => !f.CityId.HasValue || x.CityId == f.CityId));

        var filter = new SalaryFilterDto { CityId = 10 };
        var actual = (await _factSalaryService.GetFactSalariesByFilterAsync(filter)).ToList();

        Assert.All(actual, f => Assert.Equal(10, f.CityId));
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public async Task UpdateFactSalaryAsync_ValidParameters_ReturnsUpdatedFact()
    {
        var existing = new FactSalary(5,1,1,1,1,1,500m,50m);
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalaryByIdAsync(5))
            .ReturnsAsync(existing);
        _factSalaryRepositoryMock
            .Setup(r => r.UpdateFactSalaryAsync(It.IsAny<FactSalary>()))
            .Returns(Task.CompletedTask);

        var updated = await _factSalaryService.UpdateFactSalaryAsync(
            salaryFactId: 5,
            dateId:       2,
            cityId:       2,
            employerId:   2,
            jobRoleId:    2,
            employeeId:   2,
            salaryAmount: 600m,
            bonusAmount:  60m
        );

        Assert.Equal(5,    updated.SalaryFactId);
        Assert.Equal(600m, updated.SalaryAmount);
        Assert.Equal(60m,  updated.BonusAmount);
    }

    [Fact]
    public async Task UpdateFactSalaryAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factSalaryService.UpdateFactSalaryAsync(
                salaryFactId: 0,
                dateId:       1,
                cityId:       1,
                employerId:   1,
                jobRoleId:    1,
                employeeId:   1,
                salaryAmount: 100m,
                bonusAmount:  0m));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factSalaryService.UpdateFactSalaryAsync(
                salaryFactId: 1,
                dateId:       1,
                cityId:       1,
                employerId:   1,
                jobRoleId:    1,
                employeeId:   1,
                salaryAmount: -100m,
                bonusAmount:  0m));
    }

    [Fact]
    public async Task UpdateFactSalaryAsync_NotFound_ThrowsNotFoundException()
    {
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalaryByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("FactSalary 42 was not found."));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _factSalaryService.UpdateFactSalaryAsync(
                salaryFactId: 42,
                dateId:       1,
                cityId:       1,
                employerId:   1,
                jobRoleId:    1,
                employeeId:   1,
                salaryAmount: 100m,
                bonusAmount:  0m));

        Assert.Equal("FactSalary 42 was not found.", ex.Message);
    }

    [Fact]
    public async Task DeleteFactSalaryAsync_Existing_Completes()
    {
        _factSalaryRepositoryMock
            .Setup(r => r.DeleteFactSalaryByIdAsync(9))
            .Returns(Task.CompletedTask);

        await _factSalaryService.DeleteFactSalaryAsync(9);

        _factSalaryRepositoryMock.Verify(r => r.DeleteFactSalaryByIdAsync(9), Times.Once);
    }

    [Fact]
    public async Task DeleteFactSalaryAsync_NotFound_ThrowsNotFoundException()
    {
        _factSalaryRepositoryMock
            .Setup(r => r.DeleteFactSalaryByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("FactSalary 123 was not found."));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _factSalaryService.DeleteFactSalaryAsync(123));

        Assert.Equal("FactSalary 123 was not found.", ex.Message);
    }
}
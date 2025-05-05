using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Facts;

public class FactSalaryServiceUnitTests
{
    private readonly Mock<IFactSalaryRepository> _factSalaryRepositoryMock;
    private readonly Mock<ILogger<FactSalaryService>> _loggerMock;
    private readonly FactSalaryService _factSalaryService;

    public FactSalaryServiceUnitTests()
    {
        _factSalaryRepositoryMock = new Mock<IFactSalaryRepository>();
        _loggerMock = new Mock<ILogger<FactSalaryService>>();
        _factSalaryService = new FactSalaryService(_factSalaryRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateFactSalaryAsync_ValidParameters_ReturnsNewFact()
    {
        _factSalaryRepositoryMock
            .Setup(r => r.AddFactSalaryAsync(It.IsAny<FactSalary>()))
            .Callback<FactSalary>(f => f.SalaryFactId = 1)
            .Returns(Task.CompletedTask);

        var fact = await _factSalaryService.CreateFactSalaryAsync(
            dateId: 1, cityId: 2, employerId: 3, jobRoleId: 4,
            employeeId: 5, salaryAmount: 1000, bonusAmount: 100
        );

        Assert.NotNull(fact);
        Assert.Equal(1, fact.SalaryFactId);
        Assert.Equal(1000, fact.SalaryAmount);
        Assert.Equal(100, fact.BonusAmount);
    }
    
    [Fact]
    public async Task CreateFactSalaryAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factSalaryService.CreateFactSalaryAsync(
                0, 1,1,1,1,1000,100));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factSalaryService.CreateFactSalaryAsync(
                1, 1,1,1,1,-5,0));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factSalaryService.CreateFactSalaryAsync(
                1, 1,1,1,1,100, -1));
    }
    
    [Fact]
    public async Task GetFactSalaryByIdAsync_ExistingId_ReturnsFact()
    {
        var expected = new FactSalary(7,1,2,3,4,5,2000,200);
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalaryByIdAsync(7))
            .ReturnsAsync(expected);

        var actual = await _factSalaryService.GetFactSalaryByIdAsync(7);

        Assert.Same(expected, actual);
    }
    
    [Fact]
    public async Task GetFactSalaryByIdAsync_NotFound_ThrowsException()
    {
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalaryByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _factSalaryService.GetFactSalaryByIdAsync(99));
        Assert.Contains("99", ex.Message);
    }
    
    [Fact]
    public async Task GetAllFactSalariesAsync_ReturnsList()
    {
        var list = new List<FactSalary>
        {
            new FactSalary(1,1,1,1,1,1,100,0),
            new FactSalary(2,2,2,2,2,2,200,20)
        };
        _factSalaryRepositoryMock
            .Setup(r => r.GetAllFactSalariesAsync())
            .ReturnsAsync(list);

        var actual = await _factSalaryService.GetAllFactSalariesAsync();

        Assert.Equal(2, actual.Count());
    }
    
    [Fact]
    public async Task GetFactSalariesByFilterAsync_FilteredCorrectly()
    {
        var all = new[]
        {
            new FactSalary(1,1,10,1,1,1,100,0),
            new FactSalary(2,2,10,1,1,1,200,0),
            new FactSalary(3,1,20,1,1,1,300,0)
        };
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalariesByFilterAsync(It.IsAny<FactSalaryFilter>()))
            .ReturnsAsync((FactSalaryFilter filt) =>
                all.Where(f => !filt.CityId.HasValue || f.CityId == filt.CityId));

        var filter = new FactSalaryFilter { CityId = 10 };

        var actual = await _factSalaryService.GetFactSalariesByFilterAsync(filter);

        Assert.All(actual, f => Assert.Equal(10, f.CityId));
        Assert.Equal(2, actual.Count());
    }
    
    [Fact]
    public async Task UpdateFactSalaryAsync_ValidParameters_ReturnsUpdatedFact()
    {
        var existing = new FactSalary(5,1,1,1,1,1,500,50);
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalaryByIdAsync(5))
            .ReturnsAsync(existing);
        _factSalaryRepositoryMock
            .Setup(r => r.UpdateFactSalaryAsync(It.IsAny<FactSalary>()))
            .Returns(Task.CompletedTask);

        var updated = await _factSalaryService.UpdateFactSalaryAsync(
            salaryFactId: 5,
            dateId: 2, cityId: 2, employerId: 2,
            jobRoleId: 2, employeeId: 2,
            salaryAmount: 600, bonusAmount: 60
        );

        Assert.Equal(5, updated.SalaryFactId);
        Assert.Equal(600, updated.SalaryAmount);
        Assert.Equal(60, updated.BonusAmount);
    }
    
    [Fact]
    public async Task UpdateFactSalaryAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factSalaryService.UpdateFactSalaryAsync(
                salaryFactId: 0, dateId:1, cityId:1,
                employerId:1, jobRoleId:1, employeeId:1,
                salaryAmount:100, bonusAmount:0));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factSalaryService.UpdateFactSalaryAsync(
                salaryFactId:1, dateId:1, cityId:1,
                employerId:1, jobRoleId:1, employeeId:1,
                salaryAmount:-100, bonusAmount:0));
    }
    
    [Fact]
    public async Task UpdateFactSalaryAsync_NotFound_ThrowsException()
    {
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalaryByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _factSalaryService.UpdateFactSalaryAsync(
                salaryFactId: 42, dateId:1, cityId:1,
                employerId:1, jobRoleId:1, employeeId:1,
                salaryAmount:100, bonusAmount:0));
        Assert.Contains("42", ex.Message);
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
    public async Task DeleteFactSalaryAsync_NotFound_ThrowsException()
    {
        _factSalaryRepositoryMock
            .Setup(r => r.DeleteFactSalaryByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _factSalaryService.DeleteFactSalaryAsync(123));
        Assert.Contains("123", ex.Message);
    }
    
    [Fact]
    public async Task GetAverageSalaryAsync_NoMatches_ReturnsZero()
    {
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalariesByFilterAsync(It.IsAny<FactSalaryFilter>()))
            .ReturnsAsync(Array.Empty<FactSalary>());

        var avg = await _factSalaryService.GetAverageSalaryAsync(new FactSalaryFilter());

        Assert.Equal(0m, avg);
    }
    
    [Fact]
    public async Task GetAverageSalaryAsync_WithMatches_ReturnsCorrectAverage()
    {
        var list = new[]
        {
            new FactSalary(1,1,1,1,1,1,100,0),
            new FactSalary(2,1,1,1,1,1,200,0),
            new FactSalary(3,1,1,1,1,1,300,0)
        };
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalariesByFilterAsync(It.IsAny<FactSalaryFilter>()))
            .ReturnsAsync(list);

        var avg = await _factSalaryService.GetAverageSalaryAsync(new FactSalaryFilter());

        Assert.Equal(200m, avg);
    }

}
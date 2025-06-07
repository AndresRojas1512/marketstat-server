using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Services.Dimensions.DimCityService;
using MarketStat.Services.Dimensions.DimFederalDistrictService;
using MarketStat.Services.Dimensions.DimHierarchyLevelService;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using MarketStat.Services.Dimensions.DimOblastService;
using MarketStat.Services.Dimensions.DimStandardJobRoleService;
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
    
    private readonly Mock<IDimCityService> _dimCityServiceMock;
    private readonly Mock<IDimOblastService> _dimOblastServiceMock;
    private readonly Mock<IDimFederalDistrictService> _dimFederalDistrictServiceMock;
    private readonly Mock<IDimIndustryFieldService> _dimIndustryFieldServiceMock;
    private readonly Mock<IDimStandardJobRoleService> _dimStandardJobRoleServiceMock;
    private readonly Mock<IDimHierarchyLevelService> _dimHierarchyLevelServiceMock;

    private readonly FactSalaryService _factSalaryService;

    public FactSalaryServiceUnitTests()
    {
        _factSalaryRepositoryMock = new Mock<IFactSalaryRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<FactSalaryService>>();
            
        var mockDbContextOptions = new DbContextOptions<MarketStatDbContext>();
        _dbContextMock = new Mock<MarketStatDbContext>(mockDbContextOptions);

        _dimCityServiceMock = new Mock<IDimCityService>();
        _dimOblastServiceMock = new Mock<IDimOblastService>();
        _dimFederalDistrictServiceMock = new Mock<IDimFederalDistrictService>();
        _dimIndustryFieldServiceMock = new Mock<IDimIndustryFieldService>();
        _dimStandardJobRoleServiceMock = new Mock<IDimStandardJobRoleService>();
        _dimHierarchyLevelServiceMock = new Mock<IDimHierarchyLevelService>();

        _factSalaryService = new FactSalaryService(
            _factSalaryRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _dimCityServiceMock.Object,
            _dimOblastServiceMock.Object,
            _dimFederalDistrictServiceMock.Object,
            _dimIndustryFieldServiceMock.Object,
            _dimStandardJobRoleServiceMock.Object,
            _dimHierarchyLevelServiceMock.Object,
            _dbContextMock.Object
        );
    }

    private FactSalary CreateTestFactSalary(
        long salaryFactId = 0, 
        int dateId = 1, 
        int cityId = 1, 
        int employerId = 1, 
        int jobRoleId = 1, 
        int employeeId = 1, 
        decimal salaryAmount = 50000m, 
        decimal bonusAmount = 5000m)
    {
        return new FactSalary(salaryFactId, dateId, cityId, employerId, jobRoleId, employeeId, salaryAmount, bonusAmount);
    }


    [Fact]
    public async Task GetFactSalaryByIdAsync_NotFound_ThrowsNotFoundException()
    {
        long testId = 99L;
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalaryByIdAsync(testId))
            .ThrowsAsync(new NotFoundException($"FactSalary {testId} was not found."));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _factSalaryService.GetFactSalaryByIdAsync(testId));

        Assert.Equal($"FactSalary {testId} was not found.", ex.Message);
        _factSalaryRepositoryMock.Verify(r => r.GetFactSalaryByIdAsync(testId), Times.Once);
    }

    [Fact]
    public async Task UpdateFactSalaryAsync_NotFound_ThrowsNotFoundException()
    {
        long testId = 42L;
        _factSalaryRepositoryMock
            .Setup(r => r.GetFactSalaryByIdAsync(testId))
            .ThrowsAsync(new NotFoundException($"FactSalary {testId} was not found."));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _factSalaryService.UpdateFactSalaryAsync(
                salaryFactId: testId,
                dateId:       1,
                cityId:       1,
                employerId:   1,
                jobRoleId:    1,
                employeeId:   1,
                salaryAmount: 100m,
                bonusAmount:  0m));

        Assert.Equal($"FactSalary {testId} was not found.", ex.Message);
        _factSalaryRepositoryMock.Verify(r => r.GetFactSalaryByIdAsync(testId), Times.Once);
        _factSalaryRepositoryMock.Verify(r => r.UpdateFactSalaryAsync(It.IsAny<FactSalary>()), Times.Never);
    }

    [Fact]
    public async Task DeleteFactSalaryAsync_NotFound_ThrowsNotFoundException()
    {
        long testId = 123L;
        _factSalaryRepositoryMock
            .Setup(r => r.DeleteFactSalaryByIdAsync(testId)) // Use specific ID or It.IsAny<long>()
            .ThrowsAsync(new NotFoundException($"FactSalary {testId} was not found."));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _factSalaryService.DeleteFactSalaryAsync(testId));

        Assert.Equal($"FactSalary {testId} was not found.", ex.Message);
        _factSalaryRepositoryMock.Verify(r => r.DeleteFactSalaryByIdAsync(testId), Times.Once);
    }

    [Fact]
    public async Task CreateFactSalaryAsync_ValidParameters_CallsRepositoryAndReturns()
    {
        var factSalaryArg = CreateTestFactSalary();
        long expectedGeneratedId = 101L;

        _factSalaryRepositoryMock
            .Setup(r => r.AddFactSalaryAsync(It.IsAny<FactSalary>()))
            .Callback<FactSalary>(fs => fs.SalaryFactId = expectedGeneratedId)
            .Returns(Task.CompletedTask);
        
        var result = await _factSalaryService.CreateFactSalaryAsync(
            factSalaryArg.DateId, factSalaryArg.CityId, factSalaryArg.EmployerId,
            factSalaryArg.JobRoleId, factSalaryArg.EmployeeId, factSalaryArg.SalaryAmount,
            factSalaryArg.BonusAmount);

        Assert.NotNull(result);
        Assert.Equal(expectedGeneratedId, result.SalaryFactId);
        _factSalaryRepositoryMock.Verify(r => r.AddFactSalaryAsync(It.Is<FactSalary>(
            fs => fs.DateId == factSalaryArg.DateId &&
                  fs.CityId == factSalaryArg.CityId
        )), Times.Once);
    }
    
    [Fact]
    public async Task GetFactSalaryByIdAsync_Found_ReturnsEntity()
    {
        long testId = 7L;
        var expectedFact = CreateTestFactSalary(salaryFactId: testId);
        _factSalaryRepositoryMock.Setup(r => r.GetFactSalaryByIdAsync(testId))
             .ReturnsAsync(expectedFact);

        var actual = await _factSalaryService.GetFactSalaryByIdAsync(testId);
        
        Assert.Same(expectedFact, actual);
    }

    [Fact]
    public async Task GetAllFactSalariesAsync_ReturnsAllFromRepository()
    {
        var expectedList = new List<FactSalary> 
        {
            CreateTestFactSalary(1L), CreateTestFactSalary(2L)
        };
        _factSalaryRepositoryMock.Setup(r => r.GetAllFactSalariesAsync()).ReturnsAsync(expectedList);

        var result = await _factSalaryService.GetAllFactSalariesAsync();
        
        Assert.Equal(expectedList, result);
        Assert.Equal(2, result.Count());
    }
    
    [Fact]
    public async Task UpdateFactSalaryAsync_ValidParameters_UpdatesAndReturns()
    {
        long existingId = 3L;
        var existingFact = CreateTestFactSalary(salaryFactId: existingId, salaryAmount: 500m);
        var updatedSalaryAmount = 600m;

        _factSalaryRepositoryMock.Setup(r => r.GetFactSalaryByIdAsync(existingId)).ReturnsAsync(existingFact);
        _factSalaryRepositoryMock.Setup(r => r.UpdateFactSalaryAsync(It.IsAny<FactSalary>())).Returns(Task.CompletedTask);

        var updated = await _factSalaryService.UpdateFactSalaryAsync(
            existingId, existingFact.DateId, existingFact.CityId, existingFact.EmployerId,
            existingFact.JobRoleId, existingFact.EmployeeId, updatedSalaryAmount, existingFact.BonusAmount);

        Assert.NotNull(updated);
        Assert.Equal(existingId, updated.SalaryFactId);
        Assert.Equal(updatedSalaryAmount, updated.SalaryAmount);

        _factSalaryRepositoryMock.Verify(r => r.UpdateFactSalaryAsync(
            It.Is<FactSalary>(fs => fs.SalaryFactId == existingId && fs.SalaryAmount == updatedSalaryAmount)), 
            Times.Once);
    }

    [Fact]
    public async Task DeleteFactSalaryAsync_ValidId_Completes()
    {
        long testId = 8L;
        _factSalaryRepositoryMock.Setup(r => r.DeleteFactSalaryByIdAsync(testId)).Returns(Task.CompletedTask);

        await _factSalaryService.DeleteFactSalaryAsync(testId);

        _factSalaryRepositoryMock.Verify(r => r.DeleteFactSalaryByIdAsync(testId), Times.Once);
    }

}
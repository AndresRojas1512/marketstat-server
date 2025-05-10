using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimStandardJobRoleHierarchyService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimStandardJobRoleHierarchyServiceUnitTests
{
    private Mock<IDimStandardJobRoleHierarchyRepository> _dimStandardJobRoleHierarchyRepository;
    private Mock<ILogger<DimStandardJobRoleHierarchyService>> _logger;
    private DimStandardJobRoleHierarchyService _dimStandardJobRoleHierarchyService;

    public DimStandardJobRoleHierarchyServiceUnitTests()
    {
        _dimStandardJobRoleHierarchyRepository = new Mock<IDimStandardJobRoleHierarchyRepository>();
        _logger = new Mock<ILogger<DimStandardJobRoleHierarchyService>>();
        _dimStandardJobRoleHierarchyService =
            new DimStandardJobRoleHierarchyService(_dimStandardJobRoleHierarchyRepository.Object, _logger.Object);
    }
    
    [Fact]
    public async Task CreateStandardJobRoleHierarchy_Valid_CallsRepositoryAndReturnsLink()
    {
        const int jobId = 1, lvlId = 2;
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.AddStandardJobRoleHierarchyAsync(It.IsAny<DimStandardJobRoleHierarchy>()))
            .Returns(Task.CompletedTask);

        var result = await _dimStandardJobRoleHierarchyService.CreateStandardJobRoleHierarchy(jobId, lvlId);

        Assert.Equal(jobId, result.StandardJobRoleId);
        Assert.Equal(lvlId, result.HierarchyLevelId);
        _dimStandardJobRoleHierarchyRepository.Verify(r =>
            r.AddStandardJobRoleHierarchyAsync(
                It.Is<DimStandardJobRoleHierarchy>(x =>
                    x.StandardJobRoleId == jobId &&
                    x.HierarchyLevelId   == lvlId
                )), Times.Once);
    }
    
    [Fact]
    public async Task CreateStandardJobRoleHierarchy_ConflictException_IsRethrown()
    {
        const int jobId = 3, lvlId = 4;
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.AddStandardJobRoleHierarchyAsync(It.IsAny<DimStandardJobRoleHierarchy>()))
            .ThrowsAsync(new ConflictException("dup"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _dimStandardJobRoleHierarchyService.CreateStandardJobRoleHierarchy(jobId, lvlId));
    }

    [Fact]
    public async Task CreateStandardJobRoleHierarchy_OtherException_Propagates()
    {
        const int jobId = 3, lvlId = 4;
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.AddStandardJobRoleHierarchyAsync(It.IsAny<DimStandardJobRoleHierarchy>()))
            .ThrowsAsync(new InvalidOperationException("db fail"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _dimStandardJobRoleHierarchyService.CreateStandardJobRoleHierarchy(jobId, lvlId));
        Assert.Equal("db fail", ex.Message);
    }

    [Fact]
    public async Task GetStandardJobRoleHierarchyAsync_Existing_ReturnsLink()
    {
        var expected = new DimStandardJobRoleHierarchy(5, 6);
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.GetStandardJobRoleHierarchyAsync(5, 6))
            .ReturnsAsync(expected);

        var actual = await _dimStandardJobRoleHierarchyService.GetStandardJobRoleHierarchyAsync(5, 6);

        Assert.Same(expected, actual);
    }
    
    [Fact]
    public async Task GetStandardJobRoleHierarchyAsync_NotFound_IsRethrown()
    {
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.GetStandardJobRoleHierarchyAsync(7, 8))
            .ThrowsAsync(new NotFoundException("no link"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleHierarchyService.GetStandardJobRoleHierarchyAsync(7, 8));
    }
    
    [Fact]
    public async Task GetLevelsByJobRoleIdAsync_Valid_ReturnsList()
    {
        const int jobId = 10;
        var list = new[]
        {
            new DimStandardJobRoleHierarchy(jobId, 1),
            new DimStandardJobRoleHierarchy(jobId, 2)
        };
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.GetLevelsByJobRoleIdAsync(jobId))
            .ReturnsAsync(list);

        var result = (await _dimStandardJobRoleHierarchyService.GetLevelsByJobRoleIdAsync(jobId)).ToList();

        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task GetLevelsByJobRoleIdAsync_OtherException_Propagates()
    {
        const int jobId = 11;
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.GetLevelsByJobRoleIdAsync(jobId))
            .ThrowsAsync(new Exception("db error"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimStandardJobRoleHierarchyService.GetLevelsByJobRoleIdAsync(jobId));
        Assert.Equal("db error", ex.Message);
    }
    
    [Fact]
    public async Task GetJobRolesByLevelIdAsync_Valid_ReturnsList()
    {
        const int lvlId = 20;
        var list = new[]
        {
            new DimStandardJobRoleHierarchy(1, lvlId),
            new DimStandardJobRoleHierarchy(2, lvlId)
        };
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.GetJobRolesByLevelIdAsync(lvlId))
            .ReturnsAsync(list);

        var result = (await _dimStandardJobRoleHierarchyService.GetJobRolesByLevelIdAsync(lvlId)).ToList();

        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task GetJobRolesByLevelIdAsync_OtherException_Propagates()
    {
        const int lvlId = 21;
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.GetJobRolesByLevelIdAsync(lvlId))
            .ThrowsAsync(new Exception("db error"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimStandardJobRoleHierarchyService.GetJobRolesByLevelIdAsync(lvlId));
        Assert.Equal("db error", ex.Message);
    }
    
    [Fact]
    public async Task GetAllStandardJobRoleHierarchiesAsync_ReturnsList()
    {
        var list = new[]
        {
            new DimStandardJobRoleHierarchy(1,1),
            new DimStandardJobRoleHierarchy(2,2)
        };
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.GetAllStandardJobRoleHierarchiesAsync())
            .ReturnsAsync(list);

        var result = (await _dimStandardJobRoleHierarchyService.GetAllStandardJobRoleHierarchiesAsync()).ToList();

        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task DeleteStandardJobRoleHierarchyAsync_Valid_CallsRepository()
    {
        const int jobId = 30, lvlId = 31;
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.DeleteStandardJobRoleHierarchyAsync(jobId, lvlId))
            .Returns(Task.CompletedTask);

        await _dimStandardJobRoleHierarchyService.DeleteStandardJobRoleHierarchyAsync(jobId, lvlId);

        _dimStandardJobRoleHierarchyRepository.Verify(r =>
            r.DeleteStandardJobRoleHierarchyAsync(jobId, lvlId), Times.Once);
    }
    
    [Fact]
    public async Task DeleteStandardJobRoleHierarchyAsync_NotFound_IsRethrown()
    {
        const int jobId = 40, lvlId = 41;
        _dimStandardJobRoleHierarchyRepository
            .Setup(r => r.DeleteStandardJobRoleHierarchyAsync(jobId, lvlId))
            .ThrowsAsync(new NotFoundException("no link"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleHierarchyService.DeleteStandardJobRoleHierarchyAsync(jobId, lvlId));
    }
}
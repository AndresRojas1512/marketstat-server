using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimJobRoleService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimJobRoleServiceUnitTests
{
    private readonly Mock<IDimJobRoleRepository> _dimJobRoleRepositoryMock;
    private readonly Mock<ILogger<DimJobRoleService>> _loggerMock;
    private readonly DimJobRoleService _dimJobRoleService;

    public DimJobRoleServiceUnitTests()
    {
        _dimJobRoleRepositoryMock = new Mock<IDimJobRoleRepository>();
        _loggerMock = new Mock<ILogger<DimJobRoleService>>();
        _dimJobRoleService = new DimJobRoleService(_dimJobRoleRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateJobRoleAsync_ValidParameters_ReturnsNewRole()
    {
        _dimJobRoleRepositoryMock
            .Setup(r => r.GetAllJobRolesAsync())
            .ReturnsAsync(new List<DimJobRole>());

        _dimJobRoleRepositoryMock
            .Setup(r => r.AddJobRoleAsync(It.IsAny<DimJobRole>()))
            .Returns(Task.CompletedTask);
        
        var result = await _dimJobRoleService.CreateJobRoleAsync("Engineer", "Senior", 10);
        
        Assert.NotNull(result);
        Assert.Equal(1, result.JobRoleId);
        Assert.Equal("Engineer", result.JobRoleTitle);
        Assert.Equal("Senior", result.SeniorityLevel);
        Assert.Equal(10, result.IndustryFieldId);
    }

    [Fact]
    public async Task CreateJobRoleAsync_EmptyTitle_ThrowsArgumentException()
    {
        _dimJobRoleRepositoryMock
            .Setup(r => r.GetAllJobRolesAsync())
            .ReturnsAsync(new List<DimJobRole>());
        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimJobRoleService.CreateJobRoleAsync("", "Senior", 5));
    }

    [Fact]
    public async Task CreateJobRoleAsync_NonPositiveFieldId_ThrowsArgumentException()
    {
        _dimJobRoleRepositoryMock
            .Setup(r => r.GetAllJobRolesAsync())
            .ReturnsAsync(new List<DimJobRole>());
        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimJobRoleService.CreateJobRoleAsync("Engineer", "Senior", 0));
    }
    
    [Fact]
    public async Task GetJobRoleByIdAsync_ExistingId_ReturnsRole()
    {
        var existing = new DimJobRole(7, "QA", "Junior", 2);
        _dimJobRoleRepositoryMock
            .Setup(r => r.GetJobRoleByIdAsync(7))
            .ReturnsAsync(existing);
        
        var result = await _dimJobRoleService.GetJobRoleByIdAsync(7);
        
        Assert.Same(existing, result);
    }
    
    [Fact]
    public async Task GetJobRoleByIdAsync_NonExistingId_ThrowsException()
    {
        _dimJobRoleRepositoryMock
            .Setup(r => r.GetJobRoleByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException());
        
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimJobRoleService.GetJobRoleByIdAsync(99));
        Assert.Contains("99", ex.Message);
    }
    
    [Fact]
    public async Task GetAllJobRolesAsync_ReturnsList()
    {
        var list = new List<DimJobRole>
        {
            new DimJobRole(1, "A", "L1", 1),
            new DimJobRole(2, "B", "L2", 1)
        };
        _dimJobRoleRepositoryMock
            .Setup(r => r.GetAllJobRolesAsync())
            .ReturnsAsync(list);
        
        var result = await _dimJobRoleService.GetAllJobRolesAsync();
        
        Assert.Equal(2, result.Count());
        Assert.Collection(result,
            item => Assert.Equal("A", item.JobRoleTitle),
            item => Assert.Equal("B", item.JobRoleTitle)
        );
    }
    
    [Fact]
    public async Task UpdateJobRoleAsync_ValidParameters_ReturnsUpdated()
    {
        var existing = new DimJobRole(3, "Dev", "Mid", 4);
        _dimJobRoleRepositoryMock
            .Setup(r => r.GetJobRoleByIdAsync(3))
            .ReturnsAsync(existing);
        _dimJobRoleRepositoryMock
            .Setup(r => r.UpdateJobRoleAsync(It.IsAny<DimJobRole>()))
            .Returns(Task.CompletedTask);
        
        var updated = await _dimJobRoleService.UpdateJobRoleAsync(3, "DevOps", "Lead", 5);
        
        Assert.Equal(3, updated.JobRoleId);
        Assert.Equal("DevOps", updated.JobRoleTitle);
        Assert.Equal("Lead", updated.SeniorityLevel);
        Assert.Equal(5, updated.IndustryFieldId);
    }
    
    [Fact]
    public async Task DeleteJobRoleAsync_ExistingId_Completes()
    {
        _dimJobRoleRepositoryMock
            .Setup(r => r.DeleteJobRoleAsync(8))
            .Returns(Task.CompletedTask);
        
        await _dimJobRoleService.DeleteJobRoleAsync(8);
        
        _dimJobRoleRepositoryMock.Verify(r => r.DeleteJobRoleAsync(8), Times.Once);
    }
    
    [Fact]
    public async Task DeleteJobRoleAsync_NonExistingId_ThrowsException()
    {
        _dimJobRoleRepositoryMock
            .Setup(r => r.DeleteJobRoleAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException());
        
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimJobRoleService.DeleteJobRoleAsync(99));
        Assert.Contains("99", ex.Message);
    }
}
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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
            .Setup(r => r.AddJobRoleAsync(It.IsAny<DimJobRole>()))
            .Callback<DimJobRole>(r => r.JobRoleId = 1)
            .Returns(Task.CompletedTask);

        var result = await _dimJobRoleService.CreateJobRoleAsync("Engineer", standardJobRoleId: 10, hierarchyLevelId: 20);

        Assert.Equal(1, result.JobRoleId);
        Assert.Equal("Engineer", result.JobRoleTitle);
        Assert.Equal(10, result.StandardJobRoleId);
        Assert.Equal(20, result.HierarchyLevelId);

        _dimJobRoleRepositoryMock.Verify(r => r.AddJobRoleAsync(
            It.Is<DimJobRole>(d =>
                d.JobRoleId == 1 &&
                d.JobRoleTitle == "Engineer" &&
                d.StandardJobRoleId == 10 &&
                d.HierarchyLevelId == 20
            )), Times.Once);
    }

    [Theory]
    [InlineData("", 5, 5)]
    [InlineData("Role", 0, 5)]
    [InlineData("Role", 5, 0)]
    public async Task CreateJobRoleAsync_InvalidParameters_ThrowsArgumentException(
        string title, int stdId, int lvlId)
    {
        await Assert.ThrowsAsync<ArgumentException>(()
            => _dimJobRoleService.CreateJobRoleAsync(title, stdId, lvlId));
    }

    [Fact]
    public async Task CreateJobRoleAsync_RepositoryThrowsConflict_PropagatesConflictException()
    {
        _dimJobRoleRepositoryMock
            .Setup(r => r.AddJobRoleAsync(It.IsAny<DimJobRole>()))
            .ThrowsAsync(new ConflictException("duplicate"));

        var ex = await Assert.ThrowsAsync<ConflictException>(()
            => _dimJobRoleService.CreateJobRoleAsync("Dev", 1, 1));
        Assert.Equal("duplicate", ex.Message);
    }

    [Fact]
    public async Task CreateJobRoleAsync_RepositoryThrowsNotFound_PropagatesNotFoundException()
    {
        _dimJobRoleRepositoryMock
            .Setup(r => r.AddJobRoleAsync(It.IsAny<DimJobRole>()))
            .ThrowsAsync(new NotFoundException("FK not found"));

        var ex = await Assert.ThrowsAsync<NotFoundException>(()
            => _dimJobRoleService.CreateJobRoleAsync("Dev", 1, 1));
        Assert.Equal("FK not found", ex.Message);
    }

    [Fact]
    public async Task GetJobRoleByIdAsync_ExistingId_ReturnsRole()
    {
        var expected = new DimJobRole(7, "QA", 2, 2);
        _dimJobRoleRepositoryMock.Setup(r => r.GetJobRoleByIdAsync(7)).ReturnsAsync(expected);

        var actual = await _dimJobRoleService.GetJobRoleByIdAsync(7);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetJobRoleByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        _dimJobRoleRepositoryMock
            .Setup(r => r.GetJobRoleByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("JobRole 99 not found"));

        var ex = await Assert.ThrowsAsync<NotFoundException>(()
            => _dimJobRoleService.GetJobRoleByIdAsync(99));
        Assert.Equal("JobRole 99 not found", ex.Message);
    }

    [Fact]
    public async Task GetAllJobRolesAsync_ReturnsList()
    {
        var list = new List<DimJobRole>
        {
            new DimJobRole(1, "A", 1, 1),
            new DimJobRole(2, "B", 1, 1)
        };
        _dimJobRoleRepositoryMock.Setup(r => r.GetAllJobRolesAsync()).ReturnsAsync(list);

        var result = (await _dimJobRoleService.GetAllJobRolesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            item => Assert.Equal("A", item.JobRoleTitle),
            item => Assert.Equal("B", item.JobRoleTitle)
        );
    }

    [Fact]
    public async Task UpdateJobRoleAsync_ValidParameters_ReturnsUpdated()
    {
        var existing = new DimJobRole(3, "Dev", 4, 4);
        _dimJobRoleRepositoryMock.Setup(r => r.GetJobRoleByIdAsync(3)).ReturnsAsync(existing);
        _dimJobRoleRepositoryMock.Setup(r => r.UpdateJobRoleAsync(It.IsAny<DimJobRole>()))
             .Returns(Task.CompletedTask);

        var updated = await _dimJobRoleService.UpdateJobRoleAsync(3, "DevOps", 5, 5);

        Assert.Equal(3, updated.JobRoleId);
        Assert.Equal("DevOps", updated.JobRoleTitle);
        Assert.Equal(5, updated.StandardJobRoleId);
        Assert.Equal(5, updated.HierarchyLevelId);

        _dimJobRoleRepositoryMock.Verify(r => r.UpdateJobRoleAsync(
            It.Is<DimJobRole>(d =>
                d.JobRoleId == 3 &&
                d.JobRoleTitle == "DevOps" &&
                d.StandardJobRoleId == 5 &&
                d.HierarchyLevelId == 5
            )), Times.Once);
    }

    [Fact]
    public async Task UpdateJobRoleAsync_NonExistingId_ThrowsNotFoundException()
    {
        _dimJobRoleRepositoryMock
            .Setup(r => r.GetJobRoleByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("JobRole 42 not found"));

        var ex = await Assert.ThrowsAsync<NotFoundException>(()
            => _dimJobRoleService.UpdateJobRoleAsync(42, "X", 1, 1));
        Assert.Equal("JobRole 42 not found", ex.Message);
    }

    [Fact]
    public async Task UpdateJobRoleAsync_Conflict_ThrowsConflictException()
    {
        var existing = new DimJobRole(5, "Old", 1, 1);
        _dimJobRoleRepositoryMock.Setup(r => r.GetJobRoleByIdAsync(5)).ReturnsAsync(existing);
        _dimJobRoleRepositoryMock
            .Setup(r => r.UpdateJobRoleAsync(It.IsAny<DimJobRole>()))
            .ThrowsAsync(new ConflictException("duplicate"));

        var ex = await Assert.ThrowsAsync<ConflictException>(()
            => _dimJobRoleService.UpdateJobRoleAsync(5, "New", 1, 1));
        Assert.Equal("duplicate", ex.Message);
    }

    [Fact]
    public async Task DeleteJobRoleAsync_ExistingId_CallsRepository()
    {
        _dimJobRoleRepositoryMock.Setup(r => r.DeleteJobRoleAsync(8)).Returns(Task.CompletedTask);

        await _dimJobRoleService.DeleteJobRoleAsync(8);

        _dimJobRoleRepositoryMock.Verify(r => r.DeleteJobRoleAsync(8), Times.Once);
    }

    [Fact]
    public async Task DeleteJobRoleAsync_NonExistingId_ThrowsNotFoundException()
    {
        _dimJobRoleRepositoryMock
            .Setup(r => r.DeleteJobRoleAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("JobRole 99 not found"));

        var ex = await Assert.ThrowsAsync<NotFoundException>(()
            => _dimJobRoleService.DeleteJobRoleAsync(99));
        Assert.Equal("JobRole 99 not found", ex.Message);
    }
}

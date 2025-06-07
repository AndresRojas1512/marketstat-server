using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimStandardJobRoleService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimStandardJobRoleServiceUnitTests
{
    private readonly Mock<IDimStandardJobRoleRepository> _dimStandardJobRoleRepositoryMock;
    private readonly Mock<ILogger<DimStandardJobRoleService>> _loggerMock;
    private readonly DimStandardJobRoleService _dimStandardJobRoleService;

    private DimStandardJobRole CreateTestStandardJobRole(int id, string code, string title, int industryId)
    {
        return new DimStandardJobRole(id, code, title, industryId);
    }

    public DimStandardJobRoleServiceUnitTests()
    {
        _dimStandardJobRoleRepositoryMock = new Mock<IDimStandardJobRoleRepository>();
        _loggerMock = new Mock<ILogger<DimStandardJobRoleService>>();
        _dimStandardJobRoleService = new DimStandardJobRoleService(_dimStandardJobRoleRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateStandardJobRoleAsync_ValidParameters_ReturnsNewRole()
    {
        _dimStandardJobRoleRepositoryMock
            .Setup(r => r.AddStandardJobRoleAsync(It.IsAny<DimStandardJobRole>()))
            .Callback<DimStandardJobRole>(d => d.StandardJobRoleId = 1)
            .Returns(Task.CompletedTask);

        var result = await _dimStandardJobRoleService.CreateStandardJobRoleAsync("ARCH", "Architect", 2);
        
        Assert.Equal(1, result.StandardJobRoleId);
        Assert.Equal("ARCH", result.StandardJobRoleCode);
        Assert.Equal("Architect", result.StandardJobRoleTitle);
        Assert.Equal(2, result.IndustryFieldId);

        _dimStandardJobRoleRepositoryMock.Verify(r => r.AddStandardJobRoleAsync(
            It.Is<DimStandardJobRole>(d =>
                d.StandardJobRoleCode == "ARCH" &&
                d.StandardJobRoleTitle == "Architect" &&
                d.IndustryFieldId == 2
        )), Times.Once);
    }

    [Fact]
    public async Task CreateStandardJobRoleAsync_Conflict_ThrowsConflictException()
    {
        _dimStandardJobRoleRepositoryMock
            .Setup(r => r.AddStandardJobRoleAsync(It.IsAny<DimStandardJobRole>()))
            .ThrowsAsync(new ConflictException("duplicate"));
        
        await Assert.ThrowsAsync<ConflictException>(() =>
            _dimStandardJobRoleService.CreateStandardJobRoleAsync("DEV", "Developer", 1));
    }

    [Fact]
    public async Task GetStandardJobRoleByIdAsync_Existing_ReturnsRole()
    {
        var expected = CreateTestStandardJobRole(5, "ANL", "Analyst", 3);
        _dimStandardJobRoleRepositoryMock
            .Setup(r => r.GetStandardJobRoleByIdAsync(5))
            .ReturnsAsync(expected);

        var actual = await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(5);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetStandardJobRoleByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _dimStandardJobRoleRepositoryMock
            .Setup(r => r.GetStandardJobRoleByIdAsync(7))
            .ThrowsAsync(new NotFoundException("missing"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(7));
    }

    [Fact]
    public async Task GetAllStandardJobRolesAsync_ReturnsList()
    {
        var list = new List<DimStandardJobRole>
        {
            CreateTestStandardJobRole(1, "QA", "QA Engineer", 2),
            CreateTestStandardJobRole(2, "DEVOPS", "DevOps Engineer", 1)
        };
        _dimStandardJobRoleRepositoryMock
            .Setup(r => r.GetAllStandardJobRolesAsync())
            .ReturnsAsync(list);

        var result = (await _dimStandardJobRoleService.GetAllStandardJobRolesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(list, result);
    }

    [Fact]
    public async Task UpdateStandardJobRoleAsync_ValidParameters_UpdatesAndReturns()
    {
        var existing = CreateTestStandardJobRole(3, "TEST", "Tester", 2);
        _dimStandardJobRoleRepositoryMock
            .Setup(r => r.GetStandardJobRoleByIdAsync(3))
            .ReturnsAsync(existing);
        _dimStandardJobRoleRepositoryMock
            .Setup(r => r.UpdateStandardJobRoleAsync(It.IsAny<DimStandardJobRole>()))
            .Returns(Task.CompletedTask);

        var updated = await _dimStandardJobRoleService.UpdateStandardJobRoleAsync(3, "LEADTEST", "Lead Tester", 2);

        Assert.Equal(3, updated.StandardJobRoleId);
        Assert.Equal("LEADTEST", updated.StandardJobRoleCode);
        Assert.Equal("Lead Tester", updated.StandardJobRoleTitle);
        Assert.Equal(2, updated.IndustryFieldId);
        _dimStandardJobRoleRepositoryMock.Verify(r => r.UpdateStandardJobRoleAsync(
            It.Is<DimStandardJobRole>(d =>
                d.StandardJobRoleId == 3 &&
                d.StandardJobRoleCode == "LEADTEST" &&
                d.StandardJobRoleTitle == "Lead Tester" &&
                d.IndustryFieldId == 2
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateStandardJobRoleAsync_NotFound_ThrowsNotFoundException()
    {
        _dimStandardJobRoleRepositoryMock
            .Setup(r => r.GetStandardJobRoleByIdAsync(9))
            .ThrowsAsync(new NotFoundException("missing"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.UpdateStandardJobRoleAsync(9, "CODE", "Title", 1));
    }

    [Fact]
    public async Task DeleteStandardJobRoleAsync_Valid_CallsRepository()
    {
        _dimStandardJobRoleRepositoryMock
            .Setup(r => r.DeleteStandardJobRoleAsync(4))
            .Returns(Task.CompletedTask);

        await _dimStandardJobRoleService.DeleteStandardJobRoleAsync(4);

        _dimStandardJobRoleRepositoryMock.Verify(r => r.DeleteStandardJobRoleAsync(4), Times.Once);
    }

    [Fact]
    public async Task DeleteStandardJobRoleAsync_NotFound_ThrowsNotFoundException()
    {
        _dimStandardJobRoleRepositoryMock
            .Setup(r => r.DeleteStandardJobRoleAsync(6))
            .ThrowsAsync(new NotFoundException("missing"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.DeleteStandardJobRoleAsync(6));
    }
}
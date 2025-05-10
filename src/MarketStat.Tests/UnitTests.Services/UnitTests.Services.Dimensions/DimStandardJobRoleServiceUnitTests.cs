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
    private Mock<IDimStandardJobRoleRepository> _dimStandardJobRoleRepository;
    private Mock<ILogger<DimStandardJobRoleService>> _logger;
    private DimStandardJobRoleService _dimStandardJobRoleService;

    public DimStandardJobRoleServiceUnitTests()
    {
        _dimStandardJobRoleRepository = new Mock<IDimStandardJobRoleRepository>();
        _logger = new Mock<ILogger<DimStandardJobRoleService>>();
        _dimStandardJobRoleService = new DimStandardJobRoleService(_dimStandardJobRoleRepository.Object, _logger.Object);
    }
    
    [Fact]
    public async Task CreateStandardJobRoleAsync_ValidParameters_ReturnsNewRole()
    {
        _dimStandardJobRoleRepository
            .Setup(r => r.AddStandardJobRoleAsync(It.IsAny<DimStandardJobRole>()))
            .Callback<DimStandardJobRole>(d => d.StandardJobRoleId = 1)
            .Returns(Task.CompletedTask);

        var result = await _dimStandardJobRoleService.CreateStandardJobRoleAsync("Architect", 2);

        Assert.Equal(1, result.StandardJobRoleId);
        Assert.Equal("Architect", result.StandardJobRoleTitle);
        Assert.Equal(2, result.IndustryFieldId);
        _dimStandardJobRoleRepository.Verify(r => r.AddStandardJobRoleAsync(
            It.Is<DimStandardJobRole>(d =>
                d.StandardJobRoleId    == 1 &&
                d.StandardJobRoleTitle == "Architect" &&
                d.IndustryFieldId      == 2
            )), Times.Once);
    }

    [Fact]
    public async Task CreateStandardJobRoleAsync_Conflict_ThrowsConflictException()
    {
        _dimStandardJobRoleRepository
            .Setup(r => r.AddStandardJobRoleAsync(It.IsAny<DimStandardJobRole>()))
            .ThrowsAsync(new ConflictException("duplicate"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _dimStandardJobRoleService.CreateStandardJobRoleAsync("Developer", 1));
    }

    [Fact]
    public async Task CreateStandardJobRoleAsync_FkMissing_ThrowsNotFoundException()
    {
        _dimStandardJobRoleRepository
            .Setup(r => r.AddStandardJobRoleAsync(It.IsAny<DimStandardJobRole>()))
            .ThrowsAsync(new NotFoundException("fk"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.CreateStandardJobRoleAsync("Developer", 1));
    }

    [Fact]
    public async Task CreateStandardJobRoleAsync_GenericError_Propagates()
    {
        _dimStandardJobRoleRepository
            .Setup(r => r.AddStandardJobRoleAsync(It.IsAny<DimStandardJobRole>()))
            .ThrowsAsync(new InvalidOperationException("db error"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _dimStandardJobRoleService.CreateStandardJobRoleAsync("Developer", 1));
        Assert.Equal("db error", ex.Message);
    }

    [Fact]
    public async Task GetStandardJobRoleByIdAsync_Existing_ReturnsRole()
    {
        var expected = new DimStandardJobRole(5, "Analyst", 3);
        _dimStandardJobRoleRepository
            .Setup(r => r.GetStandardJobRoleByIdAsync(5))
            .ReturnsAsync(expected);

        var actual = await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(5);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetStandardJobRoleByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _dimStandardJobRoleRepository
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
            new DimStandardJobRole(1, "QA", 2),
            new DimStandardJobRole(2, "DevOps", 1)
        };
        _dimStandardJobRoleRepository
            .Setup(r => r.GetAllStandardJobRolesAsync())
            .ReturnsAsync(list);

        var result = (await _dimStandardJobRoleService.GetAllStandardJobRolesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(list, result);
    }

    [Fact]
    public async Task UpdateStandardJobRoleAsync_ValidParameters_UpdatesAndReturns()
    {
        var existing = new DimStandardJobRole(3, "Tester", 2);
        _dimStandardJobRoleRepository
            .Setup(r => r.GetStandardJobRoleByIdAsync(3))
            .ReturnsAsync(existing);
        _dimStandardJobRoleRepository
            .Setup(r => r.UpdateStandardJobRoleAsync(It.IsAny<DimStandardJobRole>()))
            .Returns(Task.CompletedTask);

        var updated = await _dimStandardJobRoleService.UpdateStandardJobRoleAsync(3, "Lead Tester", 2);

        Assert.Equal(3, updated.StandardJobRoleId);
        Assert.Equal("Lead Tester", updated.StandardJobRoleTitle);
        Assert.Equal(2, updated.IndustryFieldId);
        _dimStandardJobRoleRepository.Verify(r => r.UpdateStandardJobRoleAsync(
            It.Is<DimStandardJobRole>(d =>
                d.StandardJobRoleId    == 3 &&
                d.StandardJobRoleTitle == "Lead Tester" &&
                d.IndustryFieldId      == 2
            )), Times.Once);
    }

    [Fact]
    public async Task UpdateStandardJobRoleAsync_NotFound_ThrowsNotFoundException()
    {
        _dimStandardJobRoleRepository
            .Setup(r => r.GetStandardJobRoleByIdAsync(9))
            .ThrowsAsync(new NotFoundException("missing"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.UpdateStandardJobRoleAsync(9, "X", 1));
    }

    [Fact]
    public async Task UpdateStandardJobRoleAsync_Conflict_ThrowsConflictException()
    {
        var existing = new DimStandardJobRole(4, "Dev", 1);
        _dimStandardJobRoleRepository
            .Setup(r => r.GetStandardJobRoleByIdAsync(4))
            .ReturnsAsync(existing);
        _dimStandardJobRoleRepository
            .Setup(r => r.UpdateStandardJobRoleAsync(It.IsAny<DimStandardJobRole>()))
            .ThrowsAsync(new ConflictException("dup"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _dimStandardJobRoleService.UpdateStandardJobRoleAsync(4, "Dev", 1));
    }

    [Fact]
    public async Task DeleteStandardJobRoleAsync_Valid_CallsRepository()
    {
        _dimStandardJobRoleRepository
            .Setup(r => r.DeleteStandardJobRoleAsync(4))
            .Returns(Task.CompletedTask);

        await _dimStandardJobRoleService.DeleteStandardJobRoleAsync(4);

        _dimStandardJobRoleRepository.Verify(r => r.DeleteStandardJobRoleAsync(4), Times.Once);
    }

    [Fact]
    public async Task DeleteStandardJobRoleAsync_NotFound_ThrowsNotFoundException()
    {
        _dimStandardJobRoleRepository
            .Setup(r => r.DeleteStandardJobRoleAsync(6))
            .ThrowsAsync(new NotFoundException("missing"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.DeleteStandardJobRoleAsync(6));
    }
}
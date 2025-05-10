using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEducationService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimEducationServiceUnitTests
{
    private readonly Mock<IDimEducationRepository> _dimEducationRepositoryMock;
    private readonly Mock<ILogger<DimEducationService>> _loggerMock;
    private readonly DimEducationService _dimEducationService;

    public DimEducationServiceUnitTests()
    {
        _dimEducationRepositoryMock = new Mock<IDimEducationRepository>();
        _loggerMock = new Mock<ILogger<DimEducationService>>();
        _dimEducationService = new DimEducationService(_dimEducationRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateEducationAsync_ValidParameters_ReturnsNewDimEducation()
    {
        _dimEducationRepositoryMock
            .Setup(r => r.AddEducationAsync(It.IsAny<DimEducation>()))
            .Callback<DimEducation>(e => e.EducationId = 1)
            .Returns(Task.CompletedTask);

        var result = await _dimEducationService.CreateEducationAsync(
            "Computer Science", "09.03.04", 1, 2
        );

        Assert.Equal(1, result.EducationId);
        Assert.Equal("Computer Science", result.Specialty);
        Assert.Equal("09.03.04", result.SpecialtyCode);
        Assert.Equal(1, result.EducationLevelId);
        Assert.Equal(2, result.IndustryFieldId);

        _dimEducationRepositoryMock.Verify(r => r.AddEducationAsync(
            It.Is<DimEducation>(e =>
                e.EducationId      == 1 &&
                e.Specialty        == "Computer Science" &&
                e.SpecialtyCode    == "09.03.04" &&
                e.EducationLevelId == 1 &&
                e.IndustryFieldId  == 2
            )), Times.Once);
    }

    [Fact]
    public async Task CreateEducationAsync_Duplicate_ThrowsConflictException()
    {
        var code = "01.03.04";
        var msg  = $"An education with code '{code}' already exists.";

        _dimEducationRepositoryMock
            .Setup(r => r.AddEducationAsync(It.IsAny<DimEducation>()))
            .ThrowsAsync(new ConflictException(msg));

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _dimEducationService.CreateEducationAsync("Math", code, 3, 3)
        );

        Assert.Equal(msg, ex.Message);
    }

    [Fact]
    public async Task CreateEducationAsync_ForeignKeyViolation_ThrowsNotFoundException()
    {
        var levelId = 99;
        var fieldId = 88;
        var msg     = $"Referenced education level ({levelId}) or Industry Field ({fieldId}) not found.";

        _dimEducationRepositoryMock
            .Setup(r => r.AddEducationAsync(It.IsAny<DimEducation>()))
            .ThrowsAsync(new NotFoundException(msg));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEducationService.CreateEducationAsync("Math", "02.02.02", levelId, fieldId)
        );

        Assert.Equal(msg, ex.Message);
    }

    [Fact]
    public async Task CreateEducationAsync_NullSpecialty_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.CreateEducationAsync(null!, "01.03.01", 1, 1)
        );
    }

    [Fact]
    public async Task GetEducationByIdAsync_Existing_ReturnsDimEducation()
    {
        var expected = new DimEducation(5, "Bio", "02.03.02", 1, 1);
        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(5))
            .ReturnsAsync(expected);

        var actual = await _dimEducationService.GetEducationByIdAsync(5);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetEducationByIdAsync_NotFound_ThrowsNotFoundException()
    {
        var id  = 7;
        var msg = $"Education with ID {id} not found.";

        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(id))
            .ThrowsAsync(new NotFoundException(msg));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEducationService.GetEducationByIdAsync(id)
        );

        Assert.Equal(msg, ex.Message);
    }

    [Fact]
    public async Task GetAllEducationsAsync_ReturnsList()
    {
        var list = new List<DimEducation>
        {
            new DimEducation(1, "CS", "03.03.03", 1, 1),
            new DimEducation(2, "EE", "04.03.04", 2, 2)
        };
        _dimEducationRepositoryMock
            .Setup(r => r.GetAllEducationsAsync())
            .ReturnsAsync(list);

        var result = (await _dimEducationService.GetAllEducationsAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(list, result);
    }

    [Fact]
    public async Task UpdateEducationAsync_ValidParameters_UpdatesAndReturns()
    {
        var existing = new DimEducation(3, "Eng", "05.04.05", 1, 1);
        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(3))
            .ReturnsAsync(existing);
        _dimEducationRepositoryMock
            .Setup(r => r.UpdateEducationAsync(It.IsAny<DimEducation>()))
            .Returns(Task.CompletedTask);

        var updated = await _dimEducationService.UpdateEducationAsync(
            3, "Engineering", "06.04.06", 2, 2
        );

        Assert.Equal(3, updated.EducationId);
        Assert.Equal("Engineering", updated.Specialty);
        Assert.Equal("06.04.06", updated.SpecialtyCode);
        Assert.Equal(2, updated.EducationLevelId);
        Assert.Equal(2, updated.IndustryFieldId);

        _dimEducationRepositoryMock.Verify(r => r.UpdateEducationAsync(
            It.Is<DimEducation>(e =>
                e.EducationId      == 3 &&
                e.Specialty        == "Engineering" &&
                e.SpecialtyCode    == "06.04.06" &&
                e.EducationLevelId == 2 &&
                e.IndustryFieldId  == 2
            )), Times.Once);
    }

    [Fact]
    public async Task UpdateEducationAsync_InvalidId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.UpdateEducationAsync(0, "History", "08.04.07", 1, 1)
        );
    }

    [Fact]
    public async Task UpdateEducationAsync_NotFound_ThrowsNotFoundException()
    {
        var id  = 9;
        var msg = $"Education with ID {id} not found.";

        _dimEducationRepositoryMock
            .Setup(r => r.GetEducationByIdAsync(id))
            .ThrowsAsync(new NotFoundException(msg));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEducationService.UpdateEducationAsync(id, "X", "07.05.07", 1, 1)
        );

        Assert.Equal(msg, ex.Message);
    }

    [Fact]
    public async Task UpdateEducationAsync_InvalidSpecialty_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.UpdateEducationAsync(1, "", "03.03.03", 1, 1)
        );
    }

    [Fact]
    public async Task UpdateEducationAsync_InvalidEducationLevel_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.UpdateEducationAsync(1, "History", "04.04.04", -1, 1)
        );
    }

    [Fact]
    public async Task UpdateEducationAsync_InvalidIndustryFieldId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEducationService.UpdateEducationAsync(1, "History", "04.04.05", 1, 0)
        );
    }

    [Fact]
    public async Task DeleteEducationAsync_Existing_Completes()
    {
        _dimEducationRepositoryMock
            .Setup(r => r.DeleteEducationAsync(4))
            .Returns(Task.CompletedTask);

        await _dimEducationService.DeleteEducationAsync(4);

        _dimEducationRepositoryMock.Verify(r =>
            r.DeleteEducationAsync(4), Times.Once);
    }

    [Fact]
    public async Task DeleteEducationAsync_NotFound_ThrowsNotFoundException()
    {
        var id  = 6;
        var msg = $"Education with ID {id} not found.";

        _dimEducationRepositoryMock
            .Setup(r => r.DeleteEducationAsync(id))
            .ThrowsAsync(new NotFoundException(msg));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEducationService.DeleteEducationAsync(id)
        );

        Assert.Equal(msg, ex.Message);
    }
}
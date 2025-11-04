using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEducationService;
using MarketStat.Tests.TestData.ObjectMothers.Dimensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketStat.Services.Tests.Dimensions;

public class DimEducationServiceTests
{
    private readonly Mock<IDimEducationRepository> _mockRepository;
    private readonly Mock<ILogger<DimEducationService>> _mockLogger;
    private readonly DimEducationService _sut;

    public DimEducationServiceTests()
    {
        _mockRepository = new Mock<IDimEducationRepository>();
        _mockLogger = new Mock<ILogger<DimEducationService>>();
        _sut = new DimEducationService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateEducationAsync_ShouldCallAddEducationAsync_WhenDataIsValid()
    {
        var newEducation = DimEducationObjectMother.ANewEducation();
        _mockRepository.Setup(repo => repo.AddEducationAsync(It.IsAny<DimEducation>()))
            .Callback<DimEducation>(edu => edu.EducationId = 1)
            .Returns(Task.CompletedTask);
        var result = await _sut.CreateEducationAsync(
            newEducation.SpecialtyName,
            newEducation.SpecialtyCode,
            newEducation.EducationLevelName
        );
        _mockRepository.Verify(repo => repo.AddEducationAsync(
            It.Is<DimEducation>(e => e.SpecialtyCode == newEducation.SpecialtyCode)
        ), Times.Once);
        result.Should().NotBeNull();
        result.EducationId.Should().Be(1);
        result.SpecialtyName.Should().Be(newEducation.SpecialtyName);
    }

    [Fact]
    public async Task CreateEducationAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var newEducation = DimEducationObjectMother.ANewEducation();
        _mockRepository.Setup(repo => repo.AddEducationAsync(It.IsAny<DimEducation>()))
            .ThrowsAsync(new ConflictException("Code already exists."));
        Func<Task> act = async () => await _sut.CreateEducationAsync(
            newEducation.SpecialtyName,
            newEducation.SpecialtyCode,
            newEducation.EducationLevelName
        );
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task GetEducationByIdAsync_ShouldReturnEducation_WhenEducationExists()
    {
        var expectedEducation = DimEducationObjectMother.AnExistingEducation();
        _mockRepository.Setup(repo => repo.GetEducationByIdAsync(expectedEducation.EducationId))
            .ReturnsAsync(expectedEducation);
        var result = await _sut.GetEducationByIdAsync(expectedEducation.EducationId);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedEducation);
    }

    [Fact]
    public async Task GetEducationByIdAsync_ShouldThrowNotFoundException_WhenEducationDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetEducationByIdAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.GetEducationByIdAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetAllEducationsAsync_ShouldReturnAllEducations_WhenEducationsExist()
    {
        var expectedList = DimEducationObjectMother.SomeEducations();
        _mockRepository.Setup(repo => repo.GetAllEducationsAsync()).ReturnsAsync(expectedList);
        var result = await _sut.GetAllEducationsAsync();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedList);
    }

    [Fact]
    public async Task GetAllEducationsAsync_ShouldReturnEmptyList_WhenNoEducationsExist()
    {
        _mockRepository.Setup(repo => repo.GetAllEducationsAsync()).ReturnsAsync(new List<DimEducation>());
        var result = await _sut.GetAllEducationsAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateEducationAsync_ShouldCallUpdateEducationAsync_WhenDataIsValid()
    {
        var existingEducation = DimEducationObjectMother.AnExistingEducation();
        var updatedSpecialtyName = "Applied Physics";
        var updatedSpecialtyCode = "03.03.01";
        _mockRepository.Setup(repo => repo.GetEducationByIdAsync(existingEducation.EducationId))
                 .ReturnsAsync(existingEducation);
        _mockRepository.Setup(repo => repo.UpdateEducationAsync(It.IsAny<DimEducation>()))
                 .Returns(Task.CompletedTask);
        var result = await _sut.UpdateEducationAsync(
            existingEducation.EducationId,
            updatedSpecialtyName,
            updatedSpecialtyCode,
            existingEducation.EducationLevelName
        );
        _mockRepository.Verify(repo => repo.UpdateEducationAsync(
            It.Is<DimEducation>(e => 
                e.EducationId == existingEducation.EducationId &&
                e.SpecialtyCode == updatedSpecialtyCode
            )
        ), Times.Once);
        result.SpecialtyName.Should().Be(updatedSpecialtyName);
        result.SpecialtyCode.Should().Be(updatedSpecialtyCode);
    }

    [Fact]
    public async Task UpdateEducationAsync_ShouldThrowNotFoundException_WhenEducationDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetEducationByIdAsync(nonExistentId))
                 .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.UpdateEducationAsync(nonExistentId, "Test", "Test", "Test");
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateEducationAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var existingEducation = DimEducationObjectMother.AnExistingEducation();

        _mockRepository.Setup(repo => repo.GetEducationByIdAsync(existingEducation.EducationId))
                 .ReturnsAsync(existingEducation);
        
        _mockRepository.Setup(repo => repo.UpdateEducationAsync(It.IsAny<DimEducation>()))
                 .ThrowsAsync(new ConflictException("Conflict on update."));
        Func<Task> act = async () => await _sut.UpdateEducationAsync(
            existingEducation.EducationId, "Test", "Test", "Test"
        );
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task DeleteEducationAsync_ShouldCallDeleteEducationAsync_WhenEducationExists()
    {
        var existingId = 1;
        _mockRepository.Setup(repo => repo.DeleteEducationAsync(existingId))
            .Returns(Task.CompletedTask);
        await _sut.DeleteEducationAsync(existingId);
        _mockRepository.Verify(repo => repo.DeleteEducationAsync(existingId), Times.Once);
    }

    [Fact]
    public async Task DeleteEducationAsync_ShouldThrowNotFoundException_WhenEducationDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.DeleteEducationAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.DeleteEducationAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
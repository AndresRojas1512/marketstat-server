using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using MarketStat.Tests.TestData.ObjectMothers.Dimensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketStat.Services.Tests.Dimensions;

public class DimIndustryFieldServiceTests
{
    private readonly Mock<IDimIndustryFieldRepository> _mockRepository;
    private readonly Mock<ILogger<DimIndustryFieldService>> _mockLogger;

    private readonly DimIndustryFieldService _sut;
    
    public DimIndustryFieldServiceTests()
    {
        _mockRepository = new Mock<IDimIndustryFieldRepository>();
        _mockLogger = new Mock<ILogger<DimIndustryFieldService>>();
        _sut = new DimIndustryFieldService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateIndustryFieldAsync_ShouldCallAddIndustryFieldAsync_WhenDataIsValid()
    {
        var newField = DimIndustryFieldObjectMother.ANewIndustryField();
        _mockRepository.Setup(repo => repo.AddIndustryFieldAsync(It.IsAny<DimIndustryField>()))
            .Callback<DimIndustryField>(field => field.IndustryFieldId = 1)
            .Returns(Task.CompletedTask);
        var result = await _sut.CreateIndustryFieldAsync(
            newField.IndustryFieldCode,
            newField.IndustryFieldName
        );
        _mockRepository.Verify(repo => repo.AddIndustryFieldAsync(
            It.Is<DimIndustryField>(f => f.IndustryFieldCode == newField.IndustryFieldCode)
        ), Times.Once);
        result.Should().NotBeNull();
        result.IndustryFieldId.Should().Be(1);
        result.IndustryFieldName.Should().Be(newField.IndustryFieldName);
    }
    
    [Fact]
    public async Task CreateIndustryFieldAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var newField = DimIndustryFieldObjectMother.ANewIndustryField();
        _mockRepository.Setup(repo => repo.AddIndustryFieldAsync(It.IsAny<DimIndustryField>()))
            .ThrowsAsync(new ConflictException("Code or name already exists."));
        Func<Task> act = async () => await _sut.CreateIndustryFieldAsync(
            newField.IndustryFieldCode,
            newField.IndustryFieldName
        );
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task GetIndustryFieldByIdAsync_ShouldReturnField_WhenFieldExists()
    {
        var expectedField = DimIndustryFieldObjectMother.AnExistingIndustryField();
        _mockRepository.Setup(repo => repo.GetIndustryFieldByIdAsync(expectedField.IndustryFieldId))
            .ReturnsAsync(expectedField);
        var result = await _sut.GetIndustryFieldByIdAsync(expectedField.IndustryFieldId);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedField);
    }
    
    [Fact]
    public async Task GetIndustryFieldByIdAsync_ShouldThrowNotFoundException_WhenFieldDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetIndustryFieldByIdAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.GetIndustryFieldByIdAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetAllIndustryFieldsAsync_ShouldReturnAllFields_WhenFieldsExist()
    {
        // Arrange
        var expectedList = DimIndustryFieldObjectMother.SomeIndustryFields();
        _mockRepository.Setup(repo => repo.GetAllIndustryFieldsAsync()).ReturnsAsync(expectedList);
        var result = await _sut.GetAllIndustryFieldsAsync();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedList);
    }

    [Fact]
    public async Task GetAllIndustryFieldsAsync_ShouldReturnEmptyList_WhenNoFieldsExist()
    {
        _mockRepository.Setup(repo => repo.GetAllIndustryFieldsAsync()).ReturnsAsync(new List<DimIndustryField>());
        var result = await _sut.GetAllIndustryFieldsAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateIndustryFieldAsync_ShouldCallUpdateIndustryFieldAsync_WhenDataIsValid()
    {
        var existingField = DimIndustryFieldObjectMother.AnExistingIndustryField();
        var updatedCode = "A.01-UPD";
        var updatedName = "IT (Updated)";
        _mockRepository.Setup(repo => repo.GetIndustryFieldByIdAsync(existingField.IndustryFieldId))
                 .ReturnsAsync(existingField);
        
        _mockRepository.Setup(repo => repo.UpdateIndustryFieldAsync(It.IsAny<DimIndustryField>()))
                 .Returns(Task.CompletedTask);
        var result = await _sut.UpdateIndustryFieldAsync(
            existingField.IndustryFieldId,
            updatedCode,
            updatedName
        );
        _mockRepository.Verify(repo => repo.UpdateIndustryFieldAsync(
            It.Is<DimIndustryField>(f => 
                f.IndustryFieldId == existingField.IndustryFieldId &&
                f.IndustryFieldName == updatedName
            )
        ), Times.Once);

        result.IndustryFieldCode.Should().Be(updatedCode);
        result.IndustryFieldName.Should().Be(updatedName);
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_ShouldThrowNotFoundException_WhenFieldDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetIndustryFieldByIdAsync(nonExistentId))
                 .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.UpdateIndustryFieldAsync(nonExistentId, "Test", "Test");
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var existingField = DimIndustryFieldObjectMother.AnExistingIndustryField();
        _mockRepository.Setup(repo => repo.GetIndustryFieldByIdAsync(existingField.IndustryFieldId))
                 .ReturnsAsync(existingField);
        _mockRepository.Setup(repo => repo.UpdateIndustryFieldAsync(It.IsAny<DimIndustryField>()))
                 .ThrowsAsync(new ConflictException("Conflict on update."));
        Func<Task> act = async () => await _sut.UpdateIndustryFieldAsync(
            existingField.IndustryFieldId, "Test", "Test"
        );
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task DeleteIndustryFieldAsync_ShouldCallDeleteIndustryFieldAsync_WhenFieldExists()
    {
        var existingId = 1;
        _mockRepository.Setup(repo => repo.DeleteIndustryFieldAsync(existingId))
            .Returns(Task.CompletedTask);
        await _sut.DeleteIndustryFieldAsync(existingId);
        _mockRepository.Verify(repo => repo.DeleteIndustryFieldAsync(existingId), Times.Once);
    }

    [Fact]
    public async Task DeleteIndustryFieldAsync_ShouldThrowNotFoundException_WhenFieldDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.DeleteIndustryFieldAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.DeleteIndustryFieldAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
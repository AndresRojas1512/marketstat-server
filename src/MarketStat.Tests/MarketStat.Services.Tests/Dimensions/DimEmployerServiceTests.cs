using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;
using MarketStat.Services.Tests.TestData.Builders;
using MarketStat.Services.Tests.TestData.ObjectMothers;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketStat.Services.Tests.Dimensions;

public class DimEmployerServiceTests
{
    private readonly Mock<IDimEmployerRepository> _mockRepository;
    private readonly Mock<ILogger<DimEmployerService>> _mockLogger;

    private readonly DimEmployerService _sut;

    public DimEmployerServiceTests()
    {
        _mockRepository = new Mock<IDimEmployerRepository>();
        _mockLogger = new Mock<ILogger<DimEmployerService>>();
        _sut = new DimEmployerService(_mockRepository.Object, _mockLogger.Object);
    }
    
    
    [Fact]
    public async Task CreateEmployerAsync_ShouldCallAddEmployerAsync_WhenDataIsValid()
    {
        var newEmployer = DimEmployerObjectMother.AValidNewEmployer();
        _mockRepository.Setup(repo => repo.AddEmployerAsync(It.IsAny<DimEmployer>()))
                 .Callback<DimEmployer>(emp => emp.EmployerId = 1)
                 .Returns(Task.CompletedTask);
        var result = await _sut.CreateEmployerAsync(
            newEmployer.EmployerName,
            newEmployer.Inn,
            newEmployer.Ogrn,
            newEmployer.Kpp,
            newEmployer.RegistrationDate,
            newEmployer.LegalAddress,
            newEmployer.ContactEmail,
            newEmployer.ContactPhone,
            newEmployer.IndustryFieldId
        );
        _mockRepository.Verify(repo => repo.AddEmployerAsync(
            It.Is<DimEmployer>(e => e.EmployerName == newEmployer.EmployerName)
        ), Times.Once);
        result.Should().NotBeNull();
        result.EmployerId.Should().Be(1);
        result.EmployerName.Should().Be(newEmployer.EmployerName);
    }

    [Fact]
    public async Task CreateEmployerAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var newEmployer = DimEmployerObjectMother.AValidNewEmployer();
        _mockRepository.Setup(repo => repo.AddEmployerAsync(It.IsAny<DimEmployer>()))
                 .ThrowsAsync(new ConflictException("An employer with this name already exists."));
        Func<Task> act = async () => await _sut.CreateEmployerAsync(
            newEmployer.EmployerName,
            newEmployer.Inn,
            newEmployer.Ogrn,
            newEmployer.Kpp,
            newEmployer.RegistrationDate,
            newEmployer.LegalAddress,
            newEmployer.ContactEmail,
            newEmployer.ContactPhone,
            newEmployer.IndustryFieldId
        );
        await act.Should().ThrowAsync<ConflictException>()
                 .WithMessage("An employer with this name already exists.");
    }
    
    [Fact]
    public async Task GetEmployerByIdAsync_ShouldReturnEmployer_WhenEmployerExists()
    {
        var expectedEmployer = DimEmployerObjectMother.AnExistingEmployer();
        _mockRepository.Setup(repo => repo.GetEmployerByIdAsync(expectedEmployer.EmployerId))
            .ReturnsAsync(expectedEmployer);
        var result = await _sut.GetEmployerByIdAsync(expectedEmployer.EmployerId);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedEmployer);
    }
    
    [Fact]
    public async Task GetEmployerByIdAsync_ShouldThrowNotFoundException_WhenEmployerDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetEmployerByIdAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException($"Employer with ID {nonExistentId} not found."));
        Func<Task> act = async () => await _sut.GetEmployerByIdAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Employer with ID {nonExistentId} not found.");
    }
    
    [Fact]
    public async Task GetAllEmployersAsync_ShouldReturnAllEmployers_WhenEmployersExist()
    {
        var expectedList = DimEmployerObjectMother.SomeEmployers();
        _mockRepository.Setup(repo => repo.GetAllEmployersAsync())
            .ReturnsAsync(expectedList);
        var result = await _sut.GetAllEmployersAsync();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedList);
        result.Should().HaveCount(expectedList.Count());
    }
    
    [Fact]
    public async Task GetAllEmployersAsync_ShouldReturnEmptyList_WhenNoEmployersExist()
    {
        _mockRepository.Setup(repo => repo.GetAllEmployersAsync())
            .ReturnsAsync(new List<DimEmployer>());
        var result = await _sut.GetAllEmployersAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateEmployerAsync_ShouldCallUpdateEmployerAsync_WhenDataIsValid()
    {
        var existingEmployer = DimEmployerObjectMother.AnExistingEmployer();
        var updatedData = new DimEmployerBuilder()
            .WithName("ООО Ромашка (Updated)")
            .WithInn("1111111111")
            .Build();
        _mockRepository.Setup(repo => repo.GetEmployerByIdAsync(existingEmployer.EmployerId))
            .ReturnsAsync(existingEmployer);
        _mockRepository.Setup(repo => repo.UpdateEmployerAsync(It.IsAny<DimEmployer>()))
            .Returns(Task.CompletedTask);
        var result = await _sut.UpdateEmployerAsync(
            existingEmployer.EmployerId,
            updatedData.EmployerName,
            updatedData.Inn,
            updatedData.Ogrn,
            updatedData.Kpp,
            updatedData.RegistrationDate,
            updatedData.LegalAddress,
            updatedData.ContactEmail,
            updatedData.ContactPhone,
            updatedData.IndustryFieldId
        );

        _mockRepository.Verify(repo => repo.UpdateEmployerAsync(
            It.Is<DimEmployer>(e => e.EmployerId == existingEmployer.EmployerId && e.EmployerName == updatedData.EmployerName)
        ), Times.Once);
        
        result.Should().NotBeNull();
        result.EmployerId.Should().Be(existingEmployer.EmployerId);
        result.EmployerName.Should().Be(updatedData.EmployerName);
    }
    
    [Fact]
    public async Task UpdateEmployerAsync_ShouldThrowNotFoundException_WhenEmployerDoesNotExist()
    {
        var nonExistentId = 999;
        var updatedData = new DimEmployerBuilder().WithName("DOES NOT MATTER").Build();
        
        _mockRepository.Setup(repo => repo.GetEmployerByIdAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException($"Employer with ID {nonExistentId} not found."));
        Func<Task> act = async () => await _sut.UpdateEmployerAsync(
            nonExistentId,
            updatedData.EmployerName, updatedData.Inn, updatedData.Ogrn, updatedData.Kpp,
            updatedData.RegistrationDate, updatedData.LegalAddress, updatedData.ContactEmail,
            updatedData.ContactPhone, updatedData.IndustryFieldId
        );
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task UpdateEmployerAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var existingEmployer = DimEmployerObjectMother.AnExistingEmployer();
        _mockRepository.Setup(repo => repo.GetEmployerByIdAsync(existingEmployer.EmployerId))
            .ReturnsAsync(existingEmployer);
        _mockRepository.Setup(repo => repo.UpdateEmployerAsync(It.IsAny<DimEmployer>()))
            .ThrowsAsync(new ConflictException("Conflict on update."));
        
        var updatedData = new DimEmployerBuilder().WithName("Conflicting Name").Build();
        Func<Task> act = async () => await _sut.UpdateEmployerAsync(
            existingEmployer.EmployerId,
            updatedData.EmployerName, updatedData.Inn, updatedData.Ogrn, updatedData.Kpp,
            updatedData.RegistrationDate, updatedData.LegalAddress, updatedData.ContactEmail,
            updatedData.ContactPhone, updatedData.IndustryFieldId
        );
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task DeleteEmployerAsync_ShouldCallDeleteEmployerAsync_WhenEmployerExists()
    {
        var existingId = 1;
        _mockRepository.Setup(repo => repo.DeleteEmployerAsync(existingId))
            .Returns(Task.CompletedTask);
        await _sut.DeleteEmployerAsync(existingId);
        _mockRepository.Verify(repo => repo.DeleteEmployerAsync(existingId), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployerAsync_ShouldThrowNotFoundException_WhenEmployerDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.DeleteEmployerAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException($"Employer with ID {nonExistentId} not found."));
        Func<Task> act = async () => await _sut.DeleteEmployerAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
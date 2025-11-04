using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEducationService;
using MarketStat.Services.Dimensions.DimEmployeeService;
using MarketStat.Tests.TestData.Builders.Dimensions;
using MarketStat.Tests.TestData.ObjectMothers.Dimensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketStat.Services.Tests.Dimensions;

public class DimEmployeeServiceTests
{
    private readonly Mock<IDimEmployeeRepository> _mockRepository;
    private readonly Mock<ILogger<DimEmployeeService>> _mockLogger;

    private readonly DimEmployeeService _sut;

    public DimEmployeeServiceTests()
    {
        _mockRepository = new Mock<IDimEmployeeRepository>();
        _mockLogger = new Mock<ILogger<DimEmployeeService>>();
        _sut = new DimEmployeeService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateEmployeeAsync_ShouldCallAddEmployeeAsync_WhenDataIsValid()
    {
        var newEmployee = DimEmployeeObjectMother.ANewEmployee();
        _mockRepository.Setup(repo => repo.AddEmployeeAsync(It.IsAny<DimEmployee>()))
            .Callback<DimEmployee>(emp => emp.EmployeeId = 1)
            .Returns(Task.CompletedTask);
        var result = await _sut.CreateEmployeeAsync(
            newEmployee.EmployeeRefId,
            newEmployee.BirthDate,
            newEmployee.CareerStartDate,
            newEmployee.Gender,
            newEmployee.EducationId,
            newEmployee.GraduationYear
        );
        _mockRepository.Verify(repo => repo.AddEmployeeAsync(
            It.Is<DimEmployee>(e => e.EmployeeRefId == newEmployee.EmployeeRefId)
        ), Times.Once);
        result.Should().NotBeNull();
        result.EmployeeId.Should().Be(1);
        result.EmployeeRefId.Should().Be(newEmployee.EmployeeRefId);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var newEmployee = DimEmployeeObjectMother.ANewEmployee();
        _mockRepository.Setup(repo => repo.AddEmployeeAsync(It.IsAny<DimEmployee>()))
            .ThrowsAsync(new ConflictException("RefId already exists."));
        Func<Task> act = async () => await _sut.CreateEmployeeAsync(
            newEmployee.EmployeeRefId,
            newEmployee.BirthDate,
            newEmployee.CareerStartDate,
            newEmployee.Gender,
            newEmployee.EducationId,
            newEmployee.GraduationYear
        );
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task GetEmployeeByIdAsync_ShouldReturnEmployee_WhenEmployeeExists()
    {
        var expectedEmployee = DimEmployeeObjectMother.AnExistingEmployee();
        _mockRepository.Setup(repo => repo.GetEmployeeByIdAsync(expectedEmployee.EmployeeId))
            .ReturnsAsync(expectedEmployee);
        var result = await _sut.GetEmployeeByIdAsync(expectedEmployee.EmployeeId);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedEmployee);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_ShouldThrowNotFoundException_WhenEmployeeDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetEmployeeByIdAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.GetEmployeeByIdAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetAllEmployeesAsync_ShouldReturnAllEmployees_WhenEmployeesExist()
    {
        var expectedList = DimEmployeeObjectMother.SomeEmployees();
        _mockRepository.Setup(repo => repo.GetAllEmployeesAsync()).ReturnsAsync(expectedList);
        var result = await _sut.GetAllEmployeesAsync();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedList);
    }

    [Fact]
    public async Task GetAllEmployeesAsync_ShouldReturnEmptyList_WhenNoEmployeesExist()
    {
        _mockRepository.Setup(repo => repo.GetAllEmployeesAsync()).ReturnsAsync(new List<DimEmployee>());
        var result = await _sut.GetAllEmployeesAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateEmployeeAsync_ShouldCallUpdateEmployeeAsync_WhenDataIsValid()
    {
        var existingEmployee = DimEmployeeObjectMother.AnExistingEmployee();
        var updatedRefId = "updated-ref-id";
        _mockRepository.Setup(repo => repo.GetEmployeeByIdAsync(existingEmployee.EmployeeId))
                 .ReturnsAsync(existingEmployee);
        
        _mockRepository.Setup(repo => repo.UpdateEmployeeAsync(It.IsAny<DimEmployee>()))
                 .Returns(Task.CompletedTask);
        var result = await _sut.UpdateEmployeeAsync(
            existingEmployee.EmployeeId,
            updatedRefId,
            existingEmployee.BirthDate,
            existingEmployee.CareerStartDate,
            existingEmployee.Gender,
            existingEmployee.EducationId,
            existingEmployee.GraduationYear
        );
        _mockRepository.Verify(repo => repo.UpdateEmployeeAsync(
            It.Is<DimEmployee>(e => 
                e.EmployeeId == existingEmployee.EmployeeId &&
                e.EmployeeRefId == updatedRefId
            )
        ), Times.Once);

        result.EmployeeRefId.Should().Be(updatedRefId);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ShouldThrowNotFoundException_WhenEmployeeDoesNotExist()
    {
        var nonExistentId = 999;
        var existingEmployee = DimEmployeeObjectMother.ANewEmployee();
        _mockRepository.Setup(repo => repo.GetEmployeeByIdAsync(nonExistentId))
                 .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.UpdateEmployeeAsync(
            nonExistentId,
            existingEmployee.EmployeeRefId,
            existingEmployee.BirthDate,
            existingEmployee.CareerStartDate,
            existingEmployee.Gender,
            existingEmployee.EducationId,
            existingEmployee.GraduationYear
        );
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var existingEmployee = DimEmployeeObjectMother.AnExistingEmployee();
        _mockRepository.Setup(repo => repo.GetEmployeeByIdAsync(existingEmployee.EmployeeId))
                 .ReturnsAsync(existingEmployee);
        _mockRepository.Setup(repo => repo.UpdateEmployeeAsync(It.IsAny<DimEmployee>()))
                 .ThrowsAsync(new ConflictException("Conflict on update."));
        Func<Task> act = async () => await _sut.UpdateEmployeeAsync(
            existingEmployee.EmployeeId,
            "conflicting-ref-id",
            existingEmployee.BirthDate,
            existingEmployee.CareerStartDate,
            existingEmployee.Gender,
            existingEmployee.EducationId,
            existingEmployee.GraduationYear
        );
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task DeleteEmployeeAsync_ShouldCallDeleteEmployeeAsync_WhenEmployeeExists()
    {
        var existingId = 1;
        _mockRepository.Setup(repo => repo.DeleteEmployeeAsync(existingId))
            .Returns(Task.CompletedTask);
        await _sut.DeleteEmployeeAsync(existingId);
        _mockRepository.Verify(repo => repo.DeleteEmployeeAsync(existingId), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ShouldThrowNotFoundException_WhenEmployeeDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.DeleteEmployeeAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.DeleteEmployeeAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
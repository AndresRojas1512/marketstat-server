using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Repository.Tests.Dimensions;

[Collection("Database collection")]
public class DimEmployeeRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public DimEmployeeRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private DimEmployeeRepository CreateRepository(MarketStatDbContext context)
    {
        return new DimEmployeeRepository(context);
    }
    
    [Fact]
    public async Task AddEmployeeAsync_ShouldAddEmployee_WhenDataIsCorrect()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var newEmployee = new DimEmployeeBuilder()
            .WithId(0)
            .WithEmployeeRefId("test-ref-123")
            .Build();
        await repository.AddEmployeeAsync(newEmployee);
        newEmployee.EmployeeId.Should().BeGreaterThan(0);
        var savedEmployee = await context.DimEmployees.FindAsync(newEmployee.EmployeeId);
        savedEmployee.Should().NotBeNull();
        savedEmployee!.EmployeeRefId.Should().Be("test-ref-123");
    }

    
    [Fact]
    public async Task AddEmployeeAsync_ShouldThrowException_WhenEmployeeIsNull()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.AddEmployeeAsync(null!); 
        await act.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task GetEmployeeByIdAsync_ShouldReturnEmployee_WhenEmployeeExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var expectedEmployee = new DimEmployeeBuilder().WithId(1).Build();
        context.DimEmployees.Add(DimEmployeeConverter.ToDbModel(expectedEmployee));
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetEmployeeByIdAsync(1);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedEmployee);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_ShouldThrowNotFoundException_WhenEmployeeDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.GetEmployeeByIdAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetAllEmployeesAsync_ShouldReturnAllEmployees_WhenEmployeesExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var emp1 = DimEmployeeConverter.ToDbModel(new DimEmployeeBuilder().WithEmployeeRefId("ref1").Build());
        var emp2 = DimEmployeeConverter.ToDbModel(new DimEmployeeBuilder().WithEmployeeRefId("ref2").Build());
        context.DimEmployees.AddRange(emp1, emp2);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetAllEmployeesAsync();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllEmployeesAsync_ShouldReturnEmptyList_WhenNoEmployeesExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var result = await repository.GetAllEmployeesAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateEmployeeAsync_ShouldUpdateEmployee_WhenEmployeeExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var originalDbModel = DimEmployeeConverter.ToDbModel(
            new DimEmployeeBuilder().WithId(1).WithEmployeeRefId("old-ref").Build()
        );
        context.DimEmployees.Add(originalDbModel);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = CreateRepository(context);
        var updatedEmployee = new DimEmployeeBuilder()
            .WithId(1)
            .WithEmployeeRefId("new-ref")
            .Build();
        await repository.UpdateEmployeeAsync(updatedEmployee);
        var empInDb = await context.DimEmployees.FindAsync(1);
        empInDb.Should().NotBeNull();
        empInDb.EmployeeRefId.Should().Be("new-ref");
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ShouldThrowNotFoundException_WhenEmployeeDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var nonExistentEmployee = new DimEmployeeBuilder().WithId(999).Build();
        Func<Task> act = async () => await repository.UpdateEmployeeAsync(nonExistentEmployee);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task DeleteEmployeeAsync_ShouldDeleteEmployee_WhenEmployeeExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var employeeId = 1;
        var dbModel = DimEmployeeConverter.ToDbModel(new DimEmployeeBuilder().WithId(employeeId).Build());
        context.DimEmployees.Add(dbModel);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        (await context.DimEmployees.FindAsync(employeeId)).Should().NotBeNull();
        await repository.DeleteEmployeeAsync(employeeId);
        (await context.DimEmployees.FindAsync(employeeId)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ShouldThrowNotFoundException_WhenEmployeeDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.DeleteEmployeeAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
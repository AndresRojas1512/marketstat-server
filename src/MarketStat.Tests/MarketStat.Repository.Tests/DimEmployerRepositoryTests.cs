using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Services.Tests.TestData.Builders;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Repository.Tests;

public class DimEmployerRepositoryTests
{
    private MarketStatDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MarketStatDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new MarketStatDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private DimEmployerRepository CreateRepository(MarketStatDbContext context)
    {
        return new DimEmployerRepository(context);
    }
    
    [Fact]
    public async Task AddEmployerAsync_ShouldAddEmployer_WhenDataIsCorrect()
    {
        await using var context = CreateInMemoryDbContext();
        var repository = CreateRepository(context);
        var newEmployer = new DimEmployerBuilder()
            .WithId(0)
            .WithName("Test Corp")
            .Build();
        await repository.AddEmployerAsync(newEmployer);
        var savedEmployer = await context.DimEmployers.FindAsync(newEmployer.EmployerId);
        
        savedEmployer.Should().NotBeNull();
        savedEmployer.EmployerName.Should().Be("Test Corp");
        newEmployer.EmployerId.Should().Be(1);
    }
    
    [Fact]
    public async Task AddEmployerAsync_ShouldThrowException_WhenEmployerIsNull()
    {
        await using var context = CreateInMemoryDbContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.AddEmployerAsync(null!); 
        await act.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task GetEmployerByIdAsync_ShouldReturnEmployer_WhenEmployerExists()
    {
        await using var context = CreateInMemoryDbContext();
        
        var expectedEmployer = new DimEmployerBuilder().WithId(1).Build();
        context.DimEmployers.Add(DimEmployerConverter.ToDbModel(expectedEmployer));
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        var result = await repository.GetEmployerByIdAsync(1);
        
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedEmployer);
    }

    [Fact]
    public async Task GetEmployerByIdAsync_ShouldThrowNotFoundException_WhenEmployerDoesNotExist()
    {
        await using var context = CreateInMemoryDbContext();
        var repository = CreateRepository(context);
        var nonExistentId = 999;
        
        Func<Task> act = async () => await repository.GetEmployerByIdAsync(nonExistentId);
        
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Employer with ID {nonExistentId} not found.");
    }
    
    [Fact]
    public async Task GetAllEmployersAsync_ShouldReturnAllEmployers_WhenEmployersExist()
    {
        await using var context = CreateInMemoryDbContext();
        var employer1 = DimEmployerConverter.ToDbModel(new DimEmployerBuilder().WithName("Corp A").Build());
        var employer2 = DimEmployerConverter.ToDbModel(new DimEmployerBuilder().WithName("Corp B").Build());
        context.DimEmployers.AddRange(employer1, employer2);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetAllEmployersAsync();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().EmployerName.Should().Be("Corp A");
    }

    [Fact]
    public async Task GetAllEmployersAsync_ShouldReturnEmptyList_WhenNoEmployersExist()
    {
        await using var context = CreateInMemoryDbContext();
        var repository = CreateRepository(context);
        var result = await repository.GetAllEmployersAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateEmployerAsync_ShouldUpdateEmployer_WhenEmployerExists()
    {
        await using var context = CreateInMemoryDbContext();
        var originalDbModel = DimEmployerConverter.ToDbModel(new DimEmployerBuilder().WithId(1).WithName("Original Name").Build());
        context.DimEmployers.Add(originalDbModel);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        var repository = CreateRepository(context);
        
        var updatedEmployer = new DimEmployerBuilder()
            .WithId(1)
            .WithName("Updated Name")
            .Build();
        
        await repository.UpdateEmployerAsync(updatedEmployer);
        
        var employerInDb = await context.DimEmployers.FindAsync(1);
        employerInDb.Should().NotBeNull();
        employerInDb.EmployerName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateEmployerAsync_ShouldThrowNotFoundException_WhenEmployerDoesNotExist()
    {
        await using var context = CreateInMemoryDbContext();
        var repository = CreateRepository(context);
        
        var nonExistentEmployer = new DimEmployerBuilder().WithId(999).Build();

        Func<Task> act = async () => await repository.UpdateEmployerAsync(nonExistentEmployer);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Employer with ID {nonExistentEmployer.EmployerId} not found.");
    }
    
    [Fact]
    public async Task DeleteEmployerAsync_ShouldDeleteEmployer_WhenEmployerExists()
    {
        await using var context = CreateInMemoryDbContext();
        
        var employerId = 1;
        var dbModel = DimEmployerConverter.ToDbModel(new DimEmployerBuilder().WithId(employerId).Build());
        context.DimEmployers.Add(dbModel);
        await context.SaveChangesAsync();
        
        var repository = CreateRepository(context);
        
        (await context.DimEmployers.FindAsync(employerId)).Should().NotBeNull();

        await repository.DeleteEmployerAsync(employerId);
        
        (await context.DimEmployers.FindAsync(employerId)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteEmployerAsync_ShouldThrowNotFoundException_WhenEmployerDoesNotExist()
    {
        await using var context = CreateInMemoryDbContext();
        var repository = CreateRepository(context);
        var nonExistentId = 999;
        
        Func<Task> act = async () => await repository.DeleteEmployerAsync(nonExistentId);
        
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Employer with ID {nonExistentId} not found.");
    }
}
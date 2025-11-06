using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Repository.Tests.Dimensions;

[Collection("Database collection")]
public class DimDateRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public DimDateRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private DimDateRepository CreateRepository(MarketStatDbContext context)
    {
        return new DimDateRepository(context);
    }
    
    [Fact]
    public async Task AddDateAsync_ShouldAddDate_WhenDataIsCorrect()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var newDate = new DimDateBuilder()
            .WithId(0)
            .WithFullDate(2025, 5, 5)
            .Build();
        await repository.AddDateAsync(newDate);
        var savedDate = await context.DimDates.FindAsync(newDate.DateId);
        savedDate.Should().NotBeNull();
        savedDate.FullDate.Should().Be(new DateOnly(2025, 5, 5));
        savedDate.Year.Should().Be(2025);
        savedDate.Quarter.Should().Be(2);
        newDate.DateId.Should().Be(1);
    }

    [Fact]
    public async Task AddDateAsync_ShouldThrowException_WhenDateIsNull()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.AddDateAsync(null!);
        await act.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task GetDateByIdAsync_ShouldReturnDate_WhenDateExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var expectedDate = new DimDateBuilder().WithId(1).Build();
        context.DimDates.Add(DimDateConverter.ToDbModel(expectedDate));
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetDateByIdAsync(1);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDate);
    }

    [Fact]
    public async Task GetDateByIdAsync_ShouldThrowNotFoundException_WhenDateDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.GetDateByIdAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetAllDatesAsync_ShouldReturnAllDates_WhenDatesExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var date1 = DimDateConverter.ToDbModel(new DimDateBuilder().WithFullDate(2025, 1, 1).Build());
        var date2 = DimDateConverter.ToDbModel(new DimDateBuilder().WithFullDate(2025, 1, 2).Build());
        context.DimDates.AddRange(date1, date2);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetAllDatesAsync();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllDatesAsync_ShouldReturnEmptyList_WhenNoDatesExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var result = await repository.GetAllDatesAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateDateAsync_ShouldUpdateDate_WhenDateExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var originalDbModel = DimDateConverter.ToDbModel(
            new DimDateBuilder().WithId(1).WithFullDate(2025, 1, 1).Build() // Jan 1st (Q1)
        );
        context.DimDates.Add(originalDbModel);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = CreateRepository(context);
        var updatedDate = new DimDateBuilder()
            .WithId(1)
            .WithFullDate(2025, 8, 1)
            .Build(); 
        await repository.UpdateDateAsync(updatedDate);
        var dateInDb = await context.DimDates.FindAsync(1);
        dateInDb.Should().NotBeNull();
        dateInDb.FullDate.Should().Be(updatedDate.FullDate);
        dateInDb.Quarter.Should().Be(3);
    }

    [Fact]
    public async Task UpdateDateAsync_ShouldThrowNotFoundException_WhenDateDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var nonExistentDate = new DimDateBuilder().WithId(999).Build();
        Func<Task> act = async () => await repository.UpdateDateAsync(nonExistentDate);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task DeleteDateAsync_ShouldDeleteDate_WhenDateExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var dateId = 1;
        var dbModel = DimDateConverter.ToDbModel(new DimDateBuilder().WithId(dateId).Build());
        context.DimDates.Add(dbModel);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        (await context.DimDates.FindAsync(dateId)).Should().NotBeNull();
        await repository.DeleteDateAsync(dateId);
        (await context.DimDates.FindAsync(dateId)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteDateAsync_ShouldThrowNotFoundException_WhenDateDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.DeleteDateAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
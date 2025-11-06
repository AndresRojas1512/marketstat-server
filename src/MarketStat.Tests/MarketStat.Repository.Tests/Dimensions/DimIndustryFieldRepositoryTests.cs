using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Repository.Tests.Dimensions;

[Collection("Database collection")]
public class DimIndustryFieldRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public DimIndustryFieldRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    private DimIndustryFieldRepository CreateRepository(MarketStatDbContext context)
    {
        return new DimIndustryFieldRepository(context);
    }
    
    [Fact]
    public async Task AddIndustryFieldAsync_ShouldAddField_WhenDataIsCorrect()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var newField = new DimIndustryFieldBuilder()
            .WithId(0)
            .WithIndustryFieldCode("A.01")
            .Build();
        await repository.AddIndustryFieldAsync(newField);
        var savedField = await context.DimIndustryFields.FindAsync(newField.IndustryFieldId);
        savedField.Should().NotBeNull();
        savedField.IndustryFieldCode.Should().Be("A.01");
        newField.IndustryFieldId.Should().Be(1);
    }

    [Fact]
    public async Task AddIndustryFieldAsync_ShouldThrowException_WhenFieldIsNull()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.AddIndustryFieldAsync(null!);
        await act.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task GetIndustryFieldByIdAsync_ShouldReturnField_WhenFieldExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var expectedField = new DimIndustryFieldBuilder().WithId(1).Build();
        context.DimIndustryFields.Add(DimIndustryFieldConverter.ToDbModel(expectedField));
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetIndustryFieldByIdAsync(1);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedField);
    }

    [Fact]
    public async Task GetIndustryFieldByIdAsync_ShouldThrowNotFoundException_WhenFieldDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.GetIndustryFieldByIdAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetAllIndustryFieldsAsync_ShouldReturnAllFields_WhenFieldsExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var field1 = DimIndustryFieldConverter.ToDbModel(new DimIndustryFieldBuilder().WithIndustryFieldName("Field A").Build());
        var field2 = DimIndustryFieldConverter.ToDbModel(new DimIndustryFieldBuilder().WithIndustryFieldName("Field B").Build());
        context.DimIndustryFields.AddRange(field1, field2);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetAllIndustryFieldsAsync();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().IndustryFieldName.Should().Be("Field A");
    }

    [Fact]
    public async Task GetAllIndustryFieldsAsync_ShouldReturnEmptyList_WhenNoFieldsExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var result = await repository.GetAllIndustryFieldsAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateIndustryFieldAsync_ShouldUpdateField_WhenFieldExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var originalDbModel = DimIndustryFieldConverter.ToDbModel(
            new DimIndustryFieldBuilder().WithId(1).WithIndustryFieldName("Old Name").Build()
        );
        context.DimIndustryFields.Add(originalDbModel);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = CreateRepository(context);
        var updatedField = new DimIndustryFieldBuilder()
            .WithId(1)
            .WithIndustryFieldName("New Name")
            .Build();
        await repository.UpdateIndustryFieldAsync(updatedField);
        var fieldInDb = await context.DimIndustryFields.FindAsync(1);
        fieldInDb.Should().NotBeNull();
        fieldInDb.IndustryFieldName.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_ShouldThrowNotFoundException_WhenFieldDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var nonExistentField = new DimIndustryFieldBuilder().WithId(999).Build();
        Func<Task> act = async () => await repository.UpdateIndustryFieldAsync(nonExistentField);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task DeleteIndustryFieldAsync_ShouldDeleteField_WhenFieldExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var fieldId = 1;
        var dbModel = DimIndustryFieldConverter.ToDbModel(new DimIndustryFieldBuilder().WithId(fieldId).Build());
        context.DimIndustryFields.Add(dbModel);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        (await context.DimIndustryFields.FindAsync(fieldId)).Should().NotBeNull();
        await repository.DeleteIndustryFieldAsync(fieldId);
        (await context.DimIndustryFields.FindAsync(fieldId)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteIndustryFieldAsync_ShouldThrowNotFoundException_WhenFieldDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.DeleteIndustryFieldAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
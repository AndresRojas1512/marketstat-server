using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarketStat.Repository.Tests.Dimensions;

[Collection("Database collection")]
public class DimEducationRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public DimEducationRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private DimEducationRepository CreateRepository(MarketStatDbContext context)
    {
        return new DimEducationRepository(context);
    }
    
    [Fact]
    public async Task AddEducationAsync_ShouldAddEducation_WhenDataIsCorrect()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);

        var newEducation = new DimEducationBuilder()
            .WithId(0)
            .WithSpecialtyCode("09.03.01")
            .Build();
        await repository.AddEducationAsync(newEducation);
        newEducation.EducationId.Should().BeGreaterThan(0);
        var savedEducation = await context.DimEducations.FindAsync(newEducation.EducationId);
        savedEducation.Should().NotBeNull();
        savedEducation!.SpecialtyCode.Should().Be("09.03.01");
    }


    [Fact]
    public async Task AddEducationAsync_ShouldThrowException_WhenEducationIsNull()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.AddEducationAsync(null!);
        await act.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task GetEducationByIdAsync_ShouldReturnEducation_WhenEducationExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var expectedEducation = new DimEducationBuilder().WithId(1).Build();
        context.DimEducations.Add(DimEducationConverter.ToDbModel(expectedEducation));
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetEducationByIdAsync(1);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedEducation);
    }

    [Fact]
    public async Task GetEducationByIdAsync_ShouldThrowNotFoundException_WhenEducationDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.GetEducationByIdAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetAllEducationsAsync_ShouldReturnAllEducations_WhenEducationsExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var edu1 = DimEducationConverter.ToDbModel(new DimEducationBuilder().WithSpecialtyCode("01.01.01").Build());
        var edu2 = DimEducationConverter.ToDbModel(new DimEducationBuilder().WithSpecialtyCode("02.02.02").Build());
        context.DimEducations.AddRange(edu1, edu2);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetAllEducationsAsync();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllEducationsAsync_ShouldReturnEmptyList_WhenNoEducationsExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var result = await repository.GetAllEducationsAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateEducationAsync_ShouldUpdateEducation_WhenEducationExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var originalDbModel = DimEducationConverter.ToDbModel(
            new DimEducationBuilder().WithId(1).WithSpecialtyName("Old Name").Build()
        );
        context.DimEducations.Add(originalDbModel);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = CreateRepository(context);
        var updatedEducation = new DimEducationBuilder()
            .WithId(1)
            .WithSpecialtyName("New Name")
            .Build();
        await repository.UpdateEducationAsync(updatedEducation);
        var eduInDb = await context.DimEducations.FindAsync(1);
        eduInDb.Should().NotBeNull();
        eduInDb.SpecialtyName.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateEducationAsync_ShouldThrowNotFoundException_WhenEducationDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var nonExistentEducation = new DimEducationBuilder().WithId(999).Build();
        Func<Task> act = async () => await repository.UpdateEducationAsync(nonExistentEducation);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task DeleteEducationAsync_ShouldDeleteEducation_WhenEducationExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var educationId = 1;
        var dbModel = DimEducationConverter.ToDbModel(new DimEducationBuilder().WithId(educationId).Build());
        context.DimEducations.Add(dbModel);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        (await context.DimEducations.FindAsync(educationId)).Should().NotBeNull();
        await repository.DeleteEducationAsync(educationId);
        (await context.DimEducations.FindAsync(educationId)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteEducationAsync_ShouldThrowNotFoundException_WhenEducationDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.DeleteEducationAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
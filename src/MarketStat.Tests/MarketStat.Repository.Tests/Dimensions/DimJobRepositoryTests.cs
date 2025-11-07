using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Repository.Tests.Dimensions;

[Collection("Database collection")]
public class DimJobRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public DimJobRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private DimJobRepository CreateRepository(MarketStatDbContext context)
    {
        return new DimJobRepository(context);
    }
    
    [Fact]
    public async Task AddJobAsync_ShouldAddJob_WhenDataIsCorrect()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var newJob = new DimJobBuilder()
            .WithId(0)
            .WithStandardJobRoleTitle("Software Engineer")
            .Build();
        await repository.AddJobAsync(newJob);
        var savedJob = await context.DimJobs.FindAsync(newJob.JobId);
        savedJob.Should().NotBeNull();
        savedJob.StandardJobRoleTitle.Should().Be("Software Engineer");
        newJob.JobId.Should().Be(1);
    }
    
    [Fact]
    public async Task AddJobAsync_ShouldThrowException_WhenJobIsNull()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.AddJobAsync(null!);
        await act.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task GetJobByIdAsync_ShouldReturnJob_WhenJobExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var industryField = new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "IT", IndustryFieldCode = "A.01"};
        context.DimIndustryFields.Add(industryField);
        
        var expectedJob = new DimJobBuilder().WithId(1).WithIndustryFieldId(1).Build();
        context.DimJobs.Add(DimJobConverter.ToDbModel(expectedJob));
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetJobByIdAsync(1);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedJob, options => 
                options.Excluding(j => j.IndustryField)
        );
        result.IndustryField.Should().NotBeNull();
        result.IndustryField.IndustryFieldName.Should().Be("IT");
    }

    [Fact]
    public async Task GetJobByIdAsync_ShouldThrowNotFoundException_WhenJobDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.GetJobByIdAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetAllJobsAsync_ShouldReturnAllJobs_WhenJobsExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var industryField = new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "IT", IndustryFieldCode = "A.01"};
        context.DimIndustryFields.Add(industryField);
        var job1 = DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Job A").WithIndustryFieldId(1).Build());
        var job2 = DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Job B").WithIndustryFieldId(1).Build());
        context.DimJobs.AddRange(job1, job2);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = (await repository.GetAllJobsAsync()).ToList();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().StandardJobRoleTitle.Should().Be("Job A");
        result.First().IndustryField.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllJobsAsync_ShouldReturnEmptyList_WhenNoJobsExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var result = await repository.GetAllJobsAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateJobAsync_ShouldUpdateJob_WhenJobExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var originalDbModel = DimJobConverter.ToDbModel(
            new DimJobBuilder().WithId(1).WithStandardJobRoleTitle("Old Title").Build()
        );
        context.DimJobs.Add(originalDbModel);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = CreateRepository(context);
        var updatedJob = new DimJobBuilder()
            .WithId(1)
            .WithStandardJobRoleTitle("New Title")
            .Build(); 
        await repository.UpdateJobAsync(updatedJob);
        var jobInDb = await context.DimJobs.FindAsync(1);
        jobInDb.Should().NotBeNull();
        jobInDb.StandardJobRoleTitle.Should().Be("New Title");
    }

    [Fact]
    public async Task UpdateJobAsync_ShouldThrowNotFoundException_WhenJobDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var nonExistentJob = new DimJobBuilder().WithId(999).Build();
        Func<Task> act = async () => await repository.UpdateJobAsync(nonExistentJob);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task DeleteJobAsync_ShouldDeleteJob_WhenJobExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var jobId = 1;
        var dbModel = DimJobConverter.ToDbModel(new DimJobBuilder().WithId(jobId).Build());
        context.DimJobs.Add(dbModel);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        (await context.DimJobs.FindAsync(jobId)).Should().NotBeNull();
        await repository.DeleteJobAsync(jobId);
        (await context.DimJobs.FindAsync(jobId)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteJobAsync_ShouldThrowNotFoundException_WhenJobDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.DeleteJobAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetJobIdsByFilterAsync_ShouldReturnAllIds_WhenFiltersAreNull()
    {
        await using var context = _fixture.CreateCleanContext();
        context.DimJobs.AddRange(
            DimJobConverter.ToDbModel(new DimJobBuilder().WithId(1).Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithId(2).Build())
        );
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetJobIdsByFilterAsync(null, null, null);
        result.Should().HaveCount(2);
        result.Should().Contain(new[] { 1, 2 });
    }
    
    [Fact]
    public async Task GetJobIdsByFilterAsync_ShouldReturnFilteredIds_WhenOneFilterIsUsed()
    {
        await using var context = _fixture.CreateCleanContext();
        context.DimJobs.AddRange(
            DimJobConverter.ToDbModel(new DimJobBuilder().WithId(1).WithStandardJobRoleTitle("Engineer").Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithId(2).WithStandardJobRoleTitle("Analyst").Build())
        );
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetJobIdsByFilterAsync("Analyst", null, null);
        result.Should().HaveCount(1);
        result.Should().Contain(2);
    }
    
    [Fact]
    public async Task GetJobIdsByFilterAsync_ShouldReturnFilteredIds_WhenAllFiltersAreUsed()
    {
        await using var context = _fixture.CreateCleanContext();
        context.DimJobs.AddRange(
            DimJobConverter.ToDbModel(new DimJobBuilder().WithId(1).WithStandardJobRoleTitle("Engineer").WithHierarchyLevelName("Mid").WithIndustryFieldId(1).Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithId(2).WithStandardJobRoleTitle("Engineer").WithHierarchyLevelName("Senior").WithIndustryFieldId(1).Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithId(3).WithStandardJobRoleTitle("Engineer").WithHierarchyLevelName("Senior").WithIndustryFieldId(2).Build())
        );
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetJobIdsByFilterAsync("Engineer", "Senior", 1);
        result.Should().HaveCount(1);
        result.Should().Contain(2);
    }
    
    [Fact]
    public async Task GetDistinctStandardJobRolesAsync_ShouldReturnAllDistinctRoles_WhenFilterIsNull()
    {
        await using var context = _fixture.CreateCleanContext();
        context.DimJobs.AddRange(
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Engineer").WithIndustryFieldId(1).Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Analyst").WithIndustryFieldId(1).Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Engineer").WithIndustryFieldId(2).Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Manager").WithIndustryFieldId(2).Build())
        );
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);

        var result = (await repository.GetDistinctStandardJobRolesAsync(null)).ToList();

        result.Should().HaveCount(3);
        result.Should().ContainInOrder("Analyst", "Engineer", "Manager");
    }

    [Fact]
    public async Task GetDistinctStandardJobRolesAsync_ShouldReturnFilteredRoles_WhenIndustryIdIsProvided()
    {
        await using var context = _fixture.CreateCleanContext();
        
        context.DimIndustryFields.Add(new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "IT", IndustryFieldCode = "A.01"});
        context.DimIndustryFields.Add(new DimIndustryFieldDbModel { IndustryFieldId = 2, IndustryFieldName = "Finance", IndustryFieldCode = "B.02"});
        
        context.DimJobs.AddRange(
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Engineer").WithIndustryFieldId(1).Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Analyst").WithIndustryFieldId(1).Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Engineer").WithIndustryFieldId(2).Build()), 
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Manager").WithIndustryFieldId(2).Build())
        );
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);

        var result = (await repository.GetDistinctStandardJobRolesAsync(1)).ToList(); // Filter by Industry 1

        result.Should().HaveCount(2);
        result.Should().ContainInOrder("Analyst", "Engineer");
    }

    [Fact]
    public async Task GetDistinctHierarchyLevelsAsync_ShouldReturnAllDistinctLevels_WhenFiltersAreNull()
    {
        await using var context = _fixture.CreateCleanContext();
        context.DimJobs.AddRange(
            DimJobConverter.ToDbModel(new DimJobBuilder().WithHierarchyLevelName("Mid").Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithHierarchyLevelName("Senior").Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithHierarchyLevelName("Junior").Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithHierarchyLevelName("Mid").Build()) // Duplicate
        );
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);

        var result = (await repository.GetDistinctHierarchyLevelsAsync(null, null)).ToList();

        result.Should().HaveCount(3);
        result.Should().ContainInOrder("Junior", "Mid", "Senior");
    }

    [Fact]
    public async Task GetDistinctHierarchyLevelsAsync_ShouldReturnFilteredLevels_WhenAllFiltersAreUsed()
    {
        await using var context = _fixture.CreateCleanContext();
        
        context.DimIndustryFields.Add(new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "IT", IndustryFieldCode = "A.01"});
        context.DimIndustryFields.Add(new DimIndustryFieldDbModel { IndustryFieldId = 2, IndustryFieldName = "Finance", IndustryFieldCode = "B.02"});
        
        context.DimJobs.AddRange(
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Engineer").WithHierarchyLevelName("Mid").WithIndustryFieldId(1).Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Engineer").WithHierarchyLevelName("Senior").WithIndustryFieldId(1).Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Analyst").WithHierarchyLevelName("Mid").WithIndustryFieldId(1).Build()),
            DimJobConverter.ToDbModel(new DimJobBuilder().WithStandardJobRoleTitle("Engineer").WithHierarchyLevelName("Senior").WithIndustryFieldId(2).Build())
        );
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = (await repository.GetDistinctHierarchyLevelsAsync(1, "Engineer")).ToList(); 
        result.Should().HaveCount(2);
        result.Should().ContainInOrder("Mid", "Senior");
    }
}
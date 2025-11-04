using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimJobService;
using MarketStat.Tests.TestData.ObjectMothers.Dimensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketStat.Services.Tests.Dimensions;

public class DimJobServiceTests
{
    private readonly Mock<IDimJobRepository> _mockRepository;
    private readonly Mock<ILogger<DimJobService>> _mockLogger;
    private readonly DimJobService _sut;
    
    public DimJobServiceTests()
    {
        _mockRepository = new Mock<IDimJobRepository>();
        _mockLogger = new Mock<ILogger<DimJobService>>();
        _sut = new DimJobService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateJobAsync_ShouldCallAddJobAsync_WhenDataIsValid()
    {
        var newJob = DimJobObjectMother.ANewJob();
        _mockRepository.Setup(repo => repo.AddJobAsync(It.IsAny<DimJob>()))
            .Callback<DimJob>(job => job.JobId = 1)
            .Returns(Task.CompletedTask);
        var result = await _sut.CreateJobAsync(
            newJob.JobRoleTitle,
            newJob.StandardJobRoleTitle,
            newJob.HierarchyLevelName,
            newJob.IndustryFieldId
        );
        _mockRepository.Verify(repo => repo.AddJobAsync(
            It.Is<DimJob>(j => j.StandardJobRoleTitle == newJob.StandardJobRoleTitle)
        ), Times.Once);
        result.Should().NotBeNull();
        result.JobId.Should().Be(1);
        result.StandardJobRoleTitle.Should().Be(newJob.StandardJobRoleTitle);
    }

    [Fact]
    public async Task CreateJobAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var newJob = DimJobObjectMother.ANewJob();
        _mockRepository.Setup(repo => repo.AddJobAsync(It.IsAny<DimJob>()))
            .ThrowsAsync(new ConflictException("Job already exists."));
        Func<Task> act = async () => await _sut.CreateJobAsync(
            newJob.JobRoleTitle,
            newJob.StandardJobRoleTitle,
            newJob.HierarchyLevelName,
            newJob.IndustryFieldId
        );
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task GetJobByIdAsync_ShouldReturnJob_WhenJobExists()
    {
        var expectedJob = DimJobObjectMother.AnExistingJob();
        _mockRepository.Setup(repo => repo.GetJobByIdAsync(expectedJob.JobId))
            .ReturnsAsync(expectedJob);
        var result = await _sut.GetJobByIdAsync(expectedJob.JobId);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedJob);
    }

    [Fact]
    public async Task GetJobByIdAsync_ShouldThrowNotFoundException_WhenJobDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetJobByIdAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.GetJobByIdAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetAllJobsAsync_ShouldReturnAllJobs_WhenJobsExist()
    {
        var expectedList = DimJobObjectMother.SomeJobs();
        _mockRepository.Setup(repo => repo.GetAllJobsAsync()).ReturnsAsync(expectedList);
        var result = await _sut.GetAllJobsAsync();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedList);
    }

    [Fact]
    public async Task GetAllJobsAsync_ShouldReturnEmptyList_WhenNoJobsExist()
    {
        _mockRepository.Setup(repo => repo.GetAllJobsAsync()).ReturnsAsync(new List<DimJob>());
        var result = await _sut.GetAllJobsAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateJobAsync_ShouldCallUpdateJobAsync_WhenDataIsValid()
    {
        var existingJob = DimJobObjectMother.AnExistingJob();
        var updatedTitle = "Senior Software Engineer";
        _mockRepository.Setup(repo => repo.GetJobByIdAsync(existingJob.JobId))
                 .ReturnsAsync(existingJob);
        _mockRepository.Setup(repo => repo.UpdateJobAsync(It.IsAny<DimJob>()))
                 .Returns(Task.CompletedTask);
        var result = await _sut.UpdateJobAsync(
            existingJob.JobId,
            updatedTitle,
            updatedTitle,
            "Senior",
            existingJob.IndustryFieldId
        );
        _mockRepository.Verify(repo => repo.UpdateJobAsync(
            It.Is<DimJob>(j => 
                j.JobId == existingJob.JobId &&
                j.StandardJobRoleTitle == updatedTitle
            )
        ), Times.Once);
        result.StandardJobRoleTitle.Should().Be(updatedTitle);
        result.HierarchyLevelName.Should().Be("Senior");
    }

    [Fact]
    public async Task UpdateJobAsync_ShouldThrowNotFoundException_WhenJobDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetJobByIdAsync(nonExistentId))
                 .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.UpdateJobAsync(nonExistentId, "Test", "Test", "Test", 1);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateJobAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var existingJob = DimJobObjectMother.AnExistingJob();
        _mockRepository.Setup(repo => repo.GetJobByIdAsync(existingJob.JobId))
                 .ReturnsAsync(existingJob);
        _mockRepository.Setup(repo => repo.UpdateJobAsync(It.IsAny<DimJob>()))
                 .ThrowsAsync(new ConflictException("Conflict on update."));
        Func<Task> act = async () => await _sut.UpdateJobAsync(
            existingJob.JobId, "Test", "Test", "Test", 1
        );
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task DeleteJobAsync_ShouldCallDeleteJobAsync_WhenJobExists()
    {
        var existingId = 1;
        _mockRepository.Setup(repo => repo.DeleteJobAsync(existingId))
            .Returns(Task.CompletedTask);
        await _sut.DeleteJobAsync(existingId);
        _mockRepository.Verify(repo => repo.DeleteJobAsync(existingId), Times.Once);
    }

    [Fact]
    public async Task DeleteJobAsync_ShouldThrowNotFoundException_WhenJobDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.DeleteJobAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.DeleteJobAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
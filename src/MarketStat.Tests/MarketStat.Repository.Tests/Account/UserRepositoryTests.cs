using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Account;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Repositories.PostgresRepositories.Account;
using MarketStat.Tests.TestData.Builders.Account;

namespace MarketStat.Repository.Tests.Account;

[Collection("Database collection")]
public class UserRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    private UserRepository CreateRepository(MarketStatDbContext context)
    {
        return new UserRepository(context);
    }
    
    [Fact]
    public async Task AddUserAsync_ShouldAddUser_WhenDataIsCorrect()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var newUser = new UserBuilder()
            .WithId(0)
            .WithUsername("newuser")
            .Build();
        var result = await repository.AddUserAsync(newUser);
        var savedUser = await context.Users.FindAsync(result.UserId);
        savedUser.Should().NotBeNull();
        savedUser.Username.Should().Be("newuser");
        result.UserId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AddUserAsync_ShouldThrowException_WhenUserIsNull()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.AddUserAsync(null!);
        await act.Should().ThrowAsync<Exception>();
    }
    
    
    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnUser_WhenUserExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var expectedUser = new UserBuilder()
            .WithId(1)
            .WithUsername("findme")
            .Build();
        context.Users.Add(UserConverter.ToDbModel(expectedUser));
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetUserByUsernameAsync("findme");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.GetUserByUsernameAsync("nonexistent");
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var expectedUser = new UserBuilder().WithId(1).Build();
        context.Users.Add(UserConverter.ToDbModel(expectedUser));
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetUserByIdAsync(1);
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetUserByIdAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.GetUserByIdAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task UserExistsAsync_ShouldReturnTrue_WhenUsernameExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var existingUser = new UserBuilder().WithUsername("testuser").WithEmail("test@example.com").Build();
        context.Users.Add(UserConverter.ToDbModel(existingUser));
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.UserExistsAsync("testuser", "other@email.com");
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task UserExistsAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var result = await repository.UserExistsAsync("nonexistent", "non@existent.com");
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var originalDbModel = UserConverter.ToDbModel(
            new UserBuilder().WithId(1).WithFullName("Old Name").Build()
        );
        context.Users.Add(originalDbModel);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = CreateRepository(context);
        var updatedUser = new UserBuilder()
            .WithId(1)
            .WithFullName("New Name")
            .Build();
        await repository.UpdateUserAsync(updatedUser);
        var userInDb = await context.Users.FindAsync(1);
        userInDb.Should().NotBeNull();
        userInDb.FullName.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var nonExistentUser = new UserBuilder().WithId(999).Build();
        Func<Task> act = async () => await repository.UpdateUserAsync(nonExistentUser);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
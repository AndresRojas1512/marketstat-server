using AutoMapper;
using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Services.Auth.AuthService;
using MarketStat.Tests.TestData.ObjectMothers.Account;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketStat.Services.Tests.Account;

[Collection("Service collection")]
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    
    private readonly AuthService _sut;
    
    private readonly RegisterUserDto _registerDto;
    private readonly LoginRequestDto _loginDto;
    private readonly User _existingUser;
    private readonly UserDto _userDto;
    
    public AuthServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _sut = new AuthService(
            _mockUserRepo.Object,
            _mockConfiguration.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
        _registerDto = new RegisterUserDto
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "Password123!",
            FullName = "New User"
        };
        _loginDto = new LoginRequestDto
        {
            Username = "testuser",
            Password = "goodpassword"
        };
        _existingUser = UserObjectMother.AnExistingUser();
        _existingUser.Username = "testuser";
        _existingUser.IsActive = true;
        _existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("goodpassword");
        _userDto = new UserDto { UserId = 1, Username = "testuser", Email = "existing@example.com" };
    }
    
    private void MockJwtSettings(bool isValid = true)
    {
        var mockExpiresSection = new Mock<IConfigurationSection>();

        if (!isValid)
        {
            _mockConfiguration.Setup(c => c["JwtSettings:Key"]).Returns((string?)null);
            _mockConfiguration.Setup(c => c["JwtSettings:Issuer"]).Returns((string?)null);
            _mockConfiguration.Setup(c => c["JwtSettings:Audience"]).Returns((string?)null);

            mockExpiresSection.Setup(s => s.Value).Returns((string?)null);
            _mockConfiguration.Setup(c => c.GetSection("JwtSettings:ExpiresInMinutes")).Returns(mockExpiresSection.Object);
            return;
        }
        
        var secretKey = "a_very_long_secret_key_for_testing_32_chars"; 

        _mockConfiguration.Setup(c => c["JwtSettings:Key"]).Returns(secretKey);
        _mockConfiguration.Setup(c => c["JwtSettings:Issuer"]).Returns("test_issuer");
        _mockConfiguration.Setup(c => c["JwtSettings:Audience"]).Returns("test_audience");

        mockExpiresSection.Setup(s => s.Value).Returns("60"); // The string "60"
        _mockConfiguration.Setup(c => c.GetSection("JwtSettings:ExpiresInMinutes")).Returns(mockExpiresSection.Object);
    }
    
    [Fact]
    public async Task RegisterAsync_ShouldCreateAndReturnUser_WhenUsernameIsUnique()
    {
        _mockUserRepo.Setup(repo => repo.UserExistsAsync(_registerDto.Username, _registerDto.Email))
            .ReturnsAsync(false);
        _mockUserRepo.Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => {
                user.UserId = 1;
                return user;
            });
        _mockMapper.Setup(m => m.Map<UserDto>(It.IsAny<User>()))
            .Returns(new UserDto { UserId = 1, Username = _registerDto.Username });
        var result = await _sut.RegisterAsync(_registerDto);
        result.Should().NotBeNull();
        result.Username.Should().Be(_registerDto.Username);
        _mockUserRepo.Verify(repo => repo.AddUserAsync(It.Is<User>(u => u.Username == _registerDto.Username)), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowConflictException_WhenUserAlreadyExists()
    {
        _mockUserRepo.Setup(repo => repo.UserExistsAsync(_registerDto.Username, _registerDto.Email))
            .ReturnsAsync(true);
        Func<Task> act = async () => await _sut.RegisterAsync(_registerDto);
        await act.Should().ThrowAsync<ConflictException>();
        _mockUserRepo.Verify(repo => repo.AddUserAsync(It.IsAny<User>()), Times.Never);
    }
    
    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        MockJwtSettings();
        _mockUserRepo.Setup(repo => repo.GetUserByUsernameAsync(_loginDto.Username))
            .ReturnsAsync(_existingUser);
        _mockMapper.Setup(m => m.Map<UserDto>(_existingUser)).Returns(_userDto);
        var result = await _sut.LoginAsync(_loginDto);
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().Be(_userDto);
        _mockUserRepo.Verify(repo => repo.UpdateUserAsync(It.Is<User>(u => u.LastLoginAt.HasValue)), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowAuthenticationException_WhenUserNotFound()
    {
        _mockUserRepo.Setup(repo => repo.GetUserByUsernameAsync(_loginDto.Username))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.LoginAsync(_loginDto);
        await act.Should().ThrowAsync<Common.Exceptions.AuthenticationException>()
                 .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowAuthenticationException_WhenPasswordIsInvalid()
    {
        _mockUserRepo.Setup(repo => repo.GetUserByUsernameAsync(_loginDto.Username))
            .ReturnsAsync(_existingUser);
        var badLoginDto = new LoginRequestDto { Username = "testuser", Password = "wrongpassword" };
        Func<Task> act = async () => await _sut.LoginAsync(badLoginDto);

        await act.Should().ThrowAsync<Common.Exceptions.AuthenticationException>()
                 .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowAuthenticationException_WhenUserIsInactive()
    {
        _existingUser.IsActive = false;
        _mockUserRepo.Setup(repo => repo.GetUserByUsernameAsync(_loginDto.Username))
            .ReturnsAsync(_existingUser);
        Func<Task> act = async () => await _sut.LoginAsync(_loginDto);
        await act.Should().ThrowAsync<Common.Exceptions.AuthenticationException>()
                 .WithMessage("User account is inactive.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowApplicationException_WhenJwtKeyIsMissing()
    {
        MockJwtSettings(isValid: false);
        _mockUserRepo.Setup(repo => repo.GetUserByUsernameAsync(_loginDto.Username))
            .ReturnsAsync(_existingUser);
        Func<Task> act = async () => await _sut.LoginAsync(_loginDto);
        await act.Should().ThrowAsync<ApplicationException>()
                 .WithMessage("Authentication system configuration error. Please contact administrator.");
    }
}
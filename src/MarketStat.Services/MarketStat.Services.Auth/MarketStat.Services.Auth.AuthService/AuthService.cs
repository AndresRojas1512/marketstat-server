namespace MarketStat.Services.Auth.AuthService;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.Account.User;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Services.Auth.AuthService.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDto> RegisterAsync(RegisterUserDto registerDto)
    {
        ArgumentNullException.ThrowIfNull(registerDto);
        UserValidator.ValidateRegistration(registerDto);
        _logger.LogInformation("Attempting to register user: {Username}", registerDto.Username);

        if (await _userRepository.UserExistsAsync(registerDto.Username, registerDto.Email).ConfigureAwait(false))
        {
            _logger.LogWarning("Registration failed for {Username}: Username or email already exists.", registerDto.Username);
            throw new ConflictException("User with the same username or email already exists.");
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        var newUserDomain = new User()
        {
            Username = registerDto.Username,
            PasswordHash = passwordHash,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            IsAdmin = registerDto.IsAdmin,
        };

        var createdUserDomain = await _userRepository.AddUserAsync(newUserDomain).ConfigureAwait(false);
        _logger.LogInformation(
            "User {Username} registered successfully with ID {UserId} (IsEtlUser: {IsEtlUser})",
            createdUserDomain.Username,
            createdUserDomain.UserId,
            createdUserDomain.IsAdmin);

        return _mapper.Map<UserDto>(createdUserDomain);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto)
    {
        ArgumentNullException.ThrowIfNull(loginDto);

        UserValidator.ValidateLogin(loginDto);
        _logger.LogInformation("Attempting login for user: {Username}", loginDto.Username);

        var userDomain = await AuthenticateUserAsync(loginDto.Username, loginDto.Password).ConfigureAwait(false);

        await UpdateLastLoginSafeAsync(userDomain).ConfigureAwait(false);

        // Refactoring: Moved logic to private method to fix CA1506 coupling
        var token = GenerateJwtToken(userDomain);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "User {Username} logged in successfully. Token generated.",
            userDomain.Username);

        return new AuthResponseDto
        {
            Token = tokenString,
            Expiration = token.ValidTo,
            User = _mapper.Map<UserDto>(userDomain),
        };
    }

    public async Task<UserDto> PartialUpdateProfileAsync(int userId, string? fullName, string? email)
    {
        _logger.LogInformation("Service: Attempting to partially update profile for User {UserId}", userId);
        UserValidator.ValidateProfileUpdate(fullName, email);
        try
        {
            var existingUser = await _userRepository.GetUserByIdAsync(userId).ConfigureAwait(false);

            existingUser.FullName = fullName ?? existingUser.FullName;
            existingUser.Email = email ?? existingUser.Email;

            await _userRepository.UpdateUserAsync(existingUser).ConfigureAwait(false);
            _logger.LogInformation("Service: Successfully updated profile for User {UserId}", userId);
            return _mapper.Map<UserDto>(existingUser);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Service: Cannot update profile for user {UserId}, user not found.", userId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Service: Conflict when updating profile for user {UserId}.", userId);
            throw;
        }
    }

    private async Task<User> AuthenticateUserAsync(string username, string password)
    {
        User userDomain;
        try
        {
            userDomain = await _userRepository.GetUserByUsernameAsync(username).ConfigureAwait(false);
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Login failed for user {Username}: User not found.", username);
            throw new AuthenticationException("Invalid username or password.");
        }

        if (!userDomain.IsActive)
        {
            _logger.LogWarning("Login failed for user {Username}: Account is not active.", username);
            throw new AuthenticationException("User account is inactive.");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, userDomain.PasswordHash))
        {
            _logger.LogWarning("Login failed for user {Username}: Invalid password.", username);
            throw new AuthenticationException("Invalid username or password.");
        }

        return userDomain;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Login should proceed even if stats update fails.")]
    private async Task UpdateLastLoginSafeAsync(User user)
    {
        user.LastLoginAt = DateTimeOffset.UtcNow;
        try
        {
            await _userRepository.UpdateUserAsync(user).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating LastLoginAt for user {Username}. Login will proceed.", user.Username);
        }
    }

    private SecurityToken GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["JwtSettings:Key"];
        var jwtIssuer = _configuration["JwtSettings:Issuer"];
        var jwtAudience = _configuration["JwtSettings:Audience"];
        var jwtExpiresMinutes = _configuration.GetValue<int>("JwtSettings:ExpiresInMinutes", 60);

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
        {
            _logger.LogCritical("JWT settings (Key, Issuer, Audience) are not configured properly in appsettings.json. Cannot generate token.");
            throw new InvalidOperationException("Authentication system configuration error. Please contact administrator.");
        }

        var key = Encoding.ASCII.GetBytes(jwtKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            _logger.LogInformation("User {Username} (ID: {UserId}) assigned 'Admin' role in JWT.", user.Username, user.UserId);
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, "Analyst"));
            _logger.LogInformation("User {Username} (ID: {UserId}) assigned 'Analyst' role in JWT.", user.Username, user.UserId);
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(jwtExpiresMinutes),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}

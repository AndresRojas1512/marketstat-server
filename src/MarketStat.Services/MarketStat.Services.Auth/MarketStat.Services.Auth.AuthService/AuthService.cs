using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;
using MarketStat.Database.Core.Repositories.Account;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Exceptions;
using MarketStat.Common.Validators.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MarketStat.Services.Auth.AuthService;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User> RegisterAsync(string username, string password, string email, string fullName, bool isAdmin)
    {
        UserValidator.ValidateRegistration(username, password, email, fullName);
        _logger.LogInformation("Attempting to register user: {Username}", username);

        if (await _userRepository.UserExistsAsync(username, email))
        {
            _logger.LogWarning("Registration failed for {Username}: Username or email already exists.", username);
            throw new ConflictException("User with the same username or email already exists.");
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            
        var newUserDomain = new User() 
        {
            Username = username,
            PasswordHash = passwordHash,
            Email = email,
            FullName = fullName,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            IsAdmin = isAdmin
        };

        var createdUser = await _userRepository.AddUserAsync(newUserDomain);
        _logger.LogInformation("User {Username} registered successfully with ID {UserId} (IsAdminUser: {IsAdminUser})", 
            createdUser.Username, createdUser.UserId, createdUser.IsAdmin);
        return createdUser;
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        UserValidator.ValidateLogin(username, password);
        _logger.LogInformation("Attempting login for user: {Username}", username);

        User userDomain;
        try
        {
            userDomain = await _userRepository.GetUserByUsernameAsync(username);
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Login failed for user {Username}: User not found.", username);
            throw new Common.Exceptions.AuthenticationException("Invalid username or password.");
        }

        if (!userDomain.IsActive)
        {
            _logger.LogWarning("Login failed for user {Username}: Account is not active.", username);
            throw new Common.Exceptions.AuthenticationException("User account is inactive.");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, userDomain.PasswordHash))
        {
            _logger.LogWarning("Login failed for user {Username}: Invalid password.", username);
            throw new Common.Exceptions.AuthenticationException("Invalid username or password.");
        }

        userDomain.LastLoginAt = DateTimeOffset.UtcNow;
        try
        {
            await _userRepository.UpdateUserAsync(userDomain);
        }
        catch(Exception ex) 
        {
            _logger.LogError(ex, "Error updating LastLoginAt for user {Username}. Login will proceed.", userDomain.Username);
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = _configuration["JwtSettings:Key"];
        var jwtIssuer = _configuration["JwtSettings:Issuer"];
        var jwtAudience = _configuration["JwtSettings:Audience"];
        var jwtExpiresMinutes = _configuration.GetValue<int>("JwtSettings:ExpiresInMinutes", 60);

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
        {
            _logger.LogCritical("JWT settings (Key, Issuer, Audience) are not configured properly in appsettings.json. Cannot generate token.");
            throw new ApplicationException("Authentication system configuration error. Please contact administrator.");
        }
        
        var key = Encoding.ASCII.GetBytes(jwtKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userDomain.Username),
            new Claim(ClaimTypes.NameIdentifier, userDomain.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, userDomain.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        string roleClaimValue;
        if (userDomain.IsAdmin)
        {
            roleClaimValue = "Admin";
            claims.Add(new Claim(ClaimTypes.Role, roleClaimValue));
            _logger.LogInformation("User {Username} (ID: {UserId}) assigned 'Admin' role in JWT.", userDomain.Username, userDomain.UserId);
        }
        else
        {
            roleClaimValue = "Analyst";
            claims.Add(new Claim(ClaimTypes.Role, roleClaimValue));
            _logger.LogInformation("User {Username} (ID: {UserId}) assigned 'Analyst' role in JWT.", userDomain.Username, userDomain.UserId);
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(jwtExpiresMinutes),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation("User {Username} (RoleClaim: {UserRoleClaim}) logged in successfully. Token generated.", 
            userDomain.Username, roleClaimValue);

        return new AuthResult
        {
            Token = tokenString,
            Expiration = token.ValidTo,
            User = userDomain
        };
    }

    public async Task<User> PartialUpdateProfileAsync(int userId, string? fullName, string? email)
    {
        _logger.LogInformation("Service: Attempting to partially update profile for User {UserId}", userId);
        UserValidator.ValidateProfileUpdate(fullName, email);
        try
        {
            var existingUser = await _userRepository.GetUserByIdAsync(userId);

            existingUser.FullName = fullName ?? existingUser.FullName;
            existingUser.Email = email ?? existingUser.Email;

            await _userRepository.UpdateUserAsync(existingUser);
            _logger.LogInformation("Service: Successfully updated profile for User {UserId}", userId);
            return existingUser;
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

    public async Task<User> GetUserProfileAsync(int userId)
    {
        _logger.LogInformation("Service: Fetching profile for User {UserId}", userId);
        return await _userRepository.GetUserByIdAsync(userId);
    }
}
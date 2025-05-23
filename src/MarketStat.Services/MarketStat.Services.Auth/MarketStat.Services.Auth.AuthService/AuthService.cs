using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;
using MarketStat.Database.Core.Repositories.Account;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Exceptions;
using MarketStat.Services.Auth.AuthService.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32.SafeHandles;
using Microsoft.Extensions.Configuration;

namespace MarketStat.Services.Auth.AuthService;

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
        UserValidator.ValidateRegistration(registerDto);

        _logger.LogInformation("Attempting to register user: {Username}", registerDto.Username);

        if (await _userRepository.UserExistsAsync(registerDto.Username, registerDto.Email))
        {
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
            SavedBenchmarksCount = 0
        };

        var createdUserDomain = await _userRepository.AddUserAsync(newUserDomain);
        _logger.LogInformation("User {Username} registered successfully with ID {UserId}", createdUserDomain.Username, createdUserDomain.UserId);

        return _mapper.Map<UserDto>(createdUserDomain);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginDto)
    {
        UserValidator.ValidateLogin(loginDto); // Call validator

        _logger.LogInformation("Attempting login for user: {Username}", loginDto.Username);

        var userDomain = await _userRepository.GetUserByUsernameAsync(loginDto.Username);

        if (userDomain == null || !userDomain.IsActive)
        {
            _logger.LogWarning("Login failed for user {Username}: User not found or not active.", loginDto.Username);
            throw new MarketStat.Common.Exceptions.AuthenticationException("Invalid username or password.");
        }

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, userDomain.PasswordHash))
        {
            _logger.LogWarning("Login failed for user {Username}: Invalid password (verification failed).", loginDto.Username);
             throw new MarketStat.Common.Exceptions.AuthenticationException("Invalid username or password.");
        }

        userDomain.LastLoginAt = DateTimeOffset.UtcNow;
        await _userRepository.UpdateUserAsync(userDomain);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = _configuration["JwtSettings:Key"];
        var jwtIssuer = _configuration["JwtSettings:Issuer"];
        var jwtAudience = _configuration["JwtSettings:Audience"];
        var jwtExpiresMinutes = _configuration.GetValue<int>("JwtSettings:ExpiresInMinutes", 60);

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
        {
            _logger.LogError("JWT settings (Key, Issuer, Audience) are not configured properly.");
            throw new ApplicationException("Authentication configuration error. Please contact support.");
        }
        
        var key = Encoding.ASCII.GetBytes(jwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userDomain.Username),
                new Claim(JwtRegisteredClaimNames.NameId, userDomain.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, userDomain.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            }),
            Expires = DateTime.UtcNow.AddMinutes(jwtExpiresMinutes),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation("User {Username} logged in successfully. Token generated.", userDomain.Username);

        return new AuthResponseDto
        {
            Token = tokenString,
            Expiration = token.ValidTo,
            User = _mapper.Map<UserDto>(userDomain)
        };
    }
}
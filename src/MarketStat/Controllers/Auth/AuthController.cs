using System.Security.Claims;
using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;
using MarketStat.Services.Auth.AuthService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IMapper mapper, ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="registerDto">The registration details.</param>
    /// <returns>The created user's details.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterUserDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Registration attempt failed due to invalid model state: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Registration attempt for username: {Username}", registerDto.Username);

        var domainUser = await _authService.RegisterAsync(registerDto.Username, registerDto.Password, registerDto.Email,
            registerDto.FullName, registerDto.IsAdmin);
        var userDto = _mapper.Map<UserDto>(domainUser);
        
        _logger.LogInformation("User {Username} registered successfully with ID {UserId}", userDto.Username, userDto.UserId);
        
        return Created(string.Empty, userDto);
    }

    /// <summary>
    /// Logs in an existing user.
    /// </summary>
    /// <param name="loginDto">The login credentials.</param>
    /// <returns>An authentication response containing a JWT and user details.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Login attempt failed due to invalid model state: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Login attempt for username: {Username}", loginDto.Username);
        
        var authResult = await _authService.LoginAsync(loginDto.Username, loginDto.Password);
        var responseDto = new AuthResponseDto
        {
            Token = authResult.Token,
            Expiration = authResult.Expiration,
            User = _mapper.Map<UserDto>(authResult.User),
        };
        
        _logger.LogInformation("User {Username} logged in successfully.", loginDto.Username);
        return Ok(responseDto);
    }

    [HttpPatch("profile/me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> PartiallyUpdateProfileAsync([FromBody] PartialUpdateUserDto patchDto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
        {
            _logger.LogWarning("PartialUpdateProfile: Could not find user ID in claims.");
            return Unauthorized(new { Message = "Invalid authentication token." });
        }
        _logger.LogInformation("User {UserId} attempting to update profile", userId);
        var updatedDomainUser = await _authService.PartialUpdateProfileAsync(
            userId,
            patchDto.FullName,
            patchDto.Email
        );
        var userDto = _mapper.Map<UserDto>(updatedDomainUser);
        return Ok(userDto);
    }
}
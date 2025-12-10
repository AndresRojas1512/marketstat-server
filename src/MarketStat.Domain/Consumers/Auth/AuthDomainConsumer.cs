using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarketStat.Common.Validators.Auth;
using MarketStat.Contracts.Auth;
using MassTransit;
using Microsoft.IdentityModel.Tokens;

namespace MarketStat.Domain.Consumers.Auth;

public class AuthDomainConsumer : 
    IConsumer<ILoginRequest>,
    IConsumer<ISubmitRegisterCommand>
{
    private readonly ILogger<AuthDomainConsumer> _logger;
    private readonly IConfiguration _config;
    private readonly IRequestClient<IGetUserAuthDetailsRequest> _userParamsClient;
    private readonly IRequestClient<IPersistUserCommand> _persistUserClient;

    public AuthDomainConsumer(
        ILogger<AuthDomainConsumer> logger, 
        IConfiguration config,
        IRequestClient<IGetUserAuthDetailsRequest> userParamsClient,
        IRequestClient<IPersistUserCommand> persistUserClient)
    {
        _logger = logger;
        _config = config;
        _userParamsClient = userParamsClient;
        _persistUserClient = persistUserClient;
    }

    public async Task Consume(ConsumeContext<ILoginRequest> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Processing login for {Username}", msg.Username);

        try
        {
            var response = await _userParamsClient.GetResponse<IGetUserAuthDetailsResponse, IUserAuthDetailsNotFoundResponse>(new 
            { 
                Username = msg.Username 
            });

            if (response.Is(out Response<IUserAuthDetailsNotFoundResponse>? _))
            {
                _logger.LogWarning("Domain: Login failed. User {Username} not found.", msg.Username);
                await context.RespondAsync<ILoginFailedResponse>(new { Reason = "Invalid credentials." });
                return;
            }

            var user = response.Message as IGetUserAuthDetailsResponse;

            if (!BCrypt.Net.BCrypt.Verify(msg.Password, user!.PasswordHash))
            {
                _logger.LogWarning("Domain: Login failed. Invalid password for {Username}.", msg.Username);
                await context.RespondAsync<ILoginFailedResponse>(new { Reason = "Invalid credentials." });
                return;
            }

            if (!user.IsActive)
            {
                await context.RespondAsync<ILoginFailedResponse>(new { Reason = "Account is inactive." });
                return;
            }

            var token = GenerateJwtToken(user);

            await context.RespondAsync<ILoginResponse>(new 
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(60),
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Domain: Login error.");
            await context.RespondAsync<ILoginFailedResponse>(new { Reason = "An internal error occurred." });
        }
    }

    public async Task Consume(ConsumeContext<ISubmitRegisterCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Processing registration for {Username}", msg.Username);

        try
        {
            UserValidator.ValidateRegistration(msg.Username, msg.Password, msg.Email, msg.FullName);
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(msg.Password);
            var saveResponse = await _persistUserClient.GetResponse<IPersistUserResponse, IRegisterFailedResponse>(new 
            {
                msg.Username,
                PasswordHash = passwordHash,
                msg.Email,
                msg.FullName,
                msg.IsAdmin
            });

            if (saveResponse.Is(out Response<IRegisterFailedResponse>? failed))
            {
                await context.RespondAsync<IRegisterFailedResponse>(new { Reason = failed.Message.Reason });
                return;
            }

            if (saveResponse.Is(out Response<IPersistUserResponse>? success))
            {
                await context.RespondAsync<IRegisterResponse>(new 
                {
                    Success = true,
                    Message = "User registered successfully",
                    User = success.Message.User
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Domain: Registration error.");
            await context.RespondAsync<IRegisterFailedResponse>(new { Reason = ex.Message });
        }
    }

    private string GenerateJwtToken(IGetUserAuthDetailsResponse user)
    {
        var jwtKey = _config["JwtSettings:Key"];
        var jwtIssuer = _config["JwtSettings:Issuer"];
        var jwtAudience = _config["JwtSettings:Audience"];
        
        if (string.IsNullOrEmpty(jwtKey)) throw new Exception("JwtSettings:Key is missing");

        var key = Encoding.ASCII.GetBytes(jwtKey);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "Analyst")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
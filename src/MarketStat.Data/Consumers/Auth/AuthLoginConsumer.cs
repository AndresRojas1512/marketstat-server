using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarketStat.Contracts.Auth;
using MarketStat.Database.Core.Repositories.Account;
using MassTransit;
using Microsoft.IdentityModel.Tokens;

namespace MarketStat.Data.Consumers.Auth;

public class AuthLoginConsumer : IConsumer<ILoginRequest>
{
    private readonly IUserRepository _repository;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthLoginConsumer> _logger;

    public AuthLoginConsumer(IUserRepository repository, IConfiguration config, ILogger<AuthLoginConsumer> logger)
    {
        _repository = repository;
        _config = config;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ILoginRequest> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Attempting login for {Username}", msg.Username);
        try
        {
            var user = await _repository.GetUserByUsernameAsync(msg.Username);
            if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(msg.Password, user.PasswordHash))
            {
                await context.RespondAsync<ILoginFailedResponse>(new
                {
                    Reason = "Invalid credentials"
                });
                return;
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["JwtSettings:Key"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "Analyst")
                }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            await context.RespondAsync<ILoginResponse>(new
            {
                Token = tokenHandler.WriteToken(token),
                Expiration = token.ValidTo,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive
            });
            user.LastLoginAt = DateTime.UtcNow;
            await _repository.UpdateUserAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            await context.RespondAsync<ILoginFailedResponse>(new
            {
                Reason = "Server error"
            });
        }
    }
}
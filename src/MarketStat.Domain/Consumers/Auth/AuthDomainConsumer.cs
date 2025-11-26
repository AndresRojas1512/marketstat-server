using System.Net.Mail;
using MarketStat.Contracts.Auth;
using MassTransit;

namespace MarketStat.Domain.Consumers.Auth;

public class AuthDomainConsumer : IConsumer<ISubmitRegisterCommand>
{
    private readonly ILogger<AuthDomainConsumer> _logger;

    public AuthDomainConsumer(ILogger<AuthDomainConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ISubmitRegisterCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating Registration for {Username}...", msg.Username);
        if (string.IsNullOrWhiteSpace(msg.Username) || msg.Username.Length < 3)
        {
            _logger.LogWarning("Domain: Invalid Username.");
            return;
        }
        try
        {
            var addr = new MailAddress(msg.Email);
            if (addr.Address != msg.Email.Trim())
            {
                throw new FormatException();
            }
        }
        catch
        {
            _logger.LogWarning("Domain: Invalid Email.");
            return;
        }
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(msg.Password);
        await context.Publish<IPersistUserCommand>(new
        {
            msg.Username,
            passwordHash = passwordHash,
            msg.Email,
            msg.FullName,
            msg.IsAdmin
        });
        _logger.LogInformation("Domain: User Validated & Hashed. Forwarding to Data Service.");
    }
}
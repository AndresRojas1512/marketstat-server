using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Contracts.Auth;
using MarketStat.Database.Core.Repositories.Account;
using MassTransit;

namespace MarketStat.Data.Consumers.Auth;

public class AuthDataConsumer : IConsumer<IPersistUserCommand>
{
    private readonly IUserRepository _repository;
    private readonly ILogger<AuthDataConsumer> _logger;

    public AuthDataConsumer(IUserRepository repository, ILogger<AuthDataConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IPersistUserCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Persisting User {Username}...", msg.Username);
        if (await _repository.UserExistsAsync(msg.Username, msg.Email))
        {
            _logger.LogWarning("Data: User already exists. Skipping.");
            return;
        }

        var user = new User
        {
            Username = msg.Username,
            PasswordHash = msg.PasswordHash,
            Email = msg.Email,
            FullName = msg.FullName,
            IsAdmin = msg.IsAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _repository.AddUserAsync(user);
            _logger.LogInformation("Data: User {Id} Saved.", user.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data: Error saving user.");
        }
    }
}
using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;
using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Auth;
using MarketStat.Database.Core.Repositories.Account;
using MassTransit;

namespace MarketStat.Data.Consumers.Auth;

public class AuthDataConsumer : 
    IConsumer<IGetUserAuthDetailsRequest>,
    IConsumer<IPersistUserCommand>
{
    private readonly IUserRepository _repository;
    private readonly ILogger<AuthDataConsumer> _logger;
    private readonly IMapper _mapper;

    public AuthDataConsumer(IUserRepository repository, ILogger<AuthDataConsumer> logger, IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<IGetUserAuthDetailsRequest> context)
    {
        try
        {
            var user = await _repository.GetUserByUsernameAsync(context.Message.Username);
            
            await context.RespondAsync<IGetUserAuthDetailsResponse>(new 
            {
                user.UserId,
                user.Username,
                user.PasswordHash,
                user.Email,
                user.IsActive,
                user.IsAdmin
            });
        }
        catch (NotFoundException)
        {
            await context.RespondAsync<IUserAuthDetailsNotFoundResponse>(new 
            {
                context.Message.Username
            });
        }
    }

    public async Task Consume(ConsumeContext<IPersistUserCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Persisting new user {Username}", msg.Username);

        try
        {
            if (await _repository.UserExistsAsync(msg.Username, msg.Email))
            {
                await context.RespondAsync<IRegisterFailedResponse>(new { Reason = "Username or Email already exists." });
                return;
            }

            var newUser = new User
            {
                Username = msg.Username,
                PasswordHash = msg.PasswordHash,
                Email = msg.Email,
                FullName = msg.FullName,
                IsAdmin = msg.IsAdmin,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var createdUser = await _repository.AddUserAsync(newUser);
            var userDto = _mapper.Map<UserDto>(createdUser);

            await context.RespondAsync<IPersistUserResponse>(new { User = userDto });
        }
        catch (ConflictException ex)
        {
            await context.RespondAsync<IRegisterFailedResponse>(new { Reason = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data: Error saving user.");
            await context.RespondAsync<IRegisterFailedResponse>(new { Reason = "Database error occurred." });
        }
    }
}
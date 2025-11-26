using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;
using MarketStat.Contracts.Auth;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Gateway.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<ILoginRequest> _loginClient;

    public AuthController(IPublishEndpoint publishEndpoint, IRequestClient<ILoginRequest> loginClient)
    {
        _publishEndpoint = publishEndpoint;
        _loginClient = loginClient;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _publishEndpoint.Publish<ISubmitRegisterCommand>(new
        {
            dto.Username, dto.Password, dto.Email, dto.FullName, dto.IsAdmin
        });
        return Accepted();
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var response = await _loginClient.GetResponse<ILoginResponse, ILoginFailedResponse>(new
        {
            dto.Username, dto.Password
        });
        if (response.Is(out Response<ILoginResponse>? success))
        {
            return Ok(new AuthResponseDto
            {
                Token = success.Message.Token,
                Expiration = success.Message.Expiration,
                User = new UserDto
                {
                    Username = success.Message.Username,
                    Email = success.Message.Email,
                    IsActive = success.Message.IsActive
                }
            });
        }
        return Unauthorized(new
        {
            Message = "Invalid credentials"
        });
    }
}
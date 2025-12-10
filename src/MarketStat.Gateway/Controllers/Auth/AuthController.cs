using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;
using MarketStat.Contracts.Auth;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Gateway.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IRequestClient<ISubmitRegisterCommand> _registerClient;
    private readonly IRequestClient<ILoginRequest> _loginClient;

    public AuthController(
        IRequestClient<ISubmitRegisterCommand> registerClient,
        IRequestClient<ILoginRequest> loginClient)
    {
        _registerClient = registerClient;
        _loginClient = loginClient;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _registerClient.GetResponse<IRegisterResponse, IRegisterFailedResponse>(new
        {
            dto.Username, 
            dto.Password, 
            dto.Email, 
            dto.FullName, 
            dto.IsAdmin
        });

        if (response.Is(out Response<IRegisterResponse>? success))
        {
            return Ok(success.Message.User);
        }

        if (response.Is(out Response<IRegisterFailedResponse>? fail))
        {
            return BadRequest(new { Message = fail.Message.Reason });
        }

        return StatusCode(500, new { Message = "An internal error occurred during registration." });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _loginClient.GetResponse<ILoginResponse, ILoginFailedResponse>(new
        {
            dto.Username, 
            dto.Password
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

        if (response.Is(out Response<ILoginFailedResponse>? fail))
        {
            return Unauthorized(new { Message = fail.Message.Reason });
        }

        return Unauthorized(new { Message = "Invalid credentials" });
    }
}
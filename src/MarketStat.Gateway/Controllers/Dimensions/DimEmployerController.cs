using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;
using MarketStat.Contracts.Dimensions.DimEmployer;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Gateway.Controllers.Dimensions;

[ApiController]
[Route("api/v1/dimemployers")]
[Authorize]
public class DimEmployerController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<IGetDimEmployerRequest> _readClient;
    private readonly IRequestClient<IGetAllDimEmployersRequest> _listClient;

    public DimEmployerController(IPublishEndpoint publishEndpoint, IRequestClient<IGetDimEmployerRequest> readClient,
        IRequestClient<IGetAllDimEmployersRequest> listClient)
    {
        _publishEndpoint = publishEndpoint;
        _readClient = readClient;
        _listClient = listClient;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _listClient.GetResponse<IGetAllDimEmployersResponse>(new { });
        return Ok(response.Message.Employers);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid EmployerId");
        }
        var response = await _readClient.GetResponse<IGetDimEmployerResponse, IDimEmployerNotFoundResponse>(new
        {
            EmployerId = id
        });
        if (response.Is(out Response<IGetDimEmployerResponse>? success))
        {
            return Ok(success.Message);
        }
        return NotFound();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEmployer([FromBody] CreateDimEmployerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _publishEndpoint.Publish<ISubmitDimEmployerCommand>(new
        {
            dto.EmployerName, dto.Inn, dto.Ogrn, dto.Kpp, dto.RegistrationDate, dto.LegalAddress, dto.ContactEmail,
            dto.ContactPhone, dto.IndustryFieldId
        });
        return Accepted();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEmployer(int id, [FromBody] UpdateDimEmployerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0)
        {
            return BadRequest("Invalid EmployerId");
        }

        await _publishEndpoint.Publish<ISubmitDimEmployerUpdateCommand>(new
        {
            EmployerId = id, dto.EmployerName, dto.Inn, dto.Ogrn, dto.Kpp, dto.RegistrationDate, dto.LegalAddress,
            dto.ContactEmail, dto.ContactPhone, dto.IndustryFieldId
        });
        return Accepted();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployer(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid EmployerId");
        }
        await _publishEndpoint.Publish<ISubmitDimEmployerDeleteCommand>(new
        {
            EmployerId = id
        });
        return Accepted();
    }
}
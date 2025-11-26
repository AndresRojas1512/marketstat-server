using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployee;
using MarketStat.Contracts.Dimensions.DimEmployee;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Gateway.Controllers.Dimensions;

public class DimEmployeeController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<IGetDimEmployeeRequest> _readClient;
    private readonly IRequestClient<IGetAllDimEmployeesRequest> _listClient;
    
    public DimEmployeeController(IPublishEndpoint publishEndpoint, IRequestClient<IGetDimEmployeeRequest> readClient,
        IRequestClient<IGetAllDimEmployeesRequest> listClient)
    {
        _publishEndpoint = publishEndpoint;
        _readClient = readClient;
        _listClient = listClient;
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _listClient.GetResponse<IGetAllDimEmployeesResponse>(new { });
        return Ok(response.Message.Employees);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid ID");
        }
        var response = await _readClient.GetResponse<IGetDimEmployeeResponse, IDimEmployeeNotFoundResponse>(new
        {
            EmployeeId = id
        });
        if (response.Is(out Response<IGetDimEmployeeResponse>? success))
        {
            return Ok(success.Message);
        }
        return NotFound();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateDimEmployeeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _publishEndpoint.Publish<ISubmitDimEmployeeCommand>(new 
        {
            dto.EmployeeRefId, dto.BirthDate, dto.CareerStartDate, dto.Gender, dto.EducationId, dto.GraduationYear
        });
        return Accepted();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateDimEmployeeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id <= 0)
        {
            return BadRequest("Invalid ID");
        }
        await _publishEndpoint.Publish<ISubmitDimEmployeeUpdateCommand>(new 
        {
            EmployeeId = id,
            dto.EmployeeRefId, dto.BirthDate, dto.CareerStartDate, dto.Gender, dto.EducationId, dto.GraduationYear
        });
        return Accepted();
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PatchEmployee(int id, [FromBody] PartialUpdateDimEmployeeDto dto)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid ID");
        }
        await _publishEndpoint.Publish<ISubmitDimEmployeePartialUpdateCommand>(new 
        {
            EmployeeId = id,
            dto.EmployeeRefId, dto.CareerStartDate, dto.EducationId, dto.GraduationYear
        });
        return Accepted();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid ID");
        }
        await _publishEndpoint.Publish<ISubmitDimEmployeeDeleteCommand>(new
        {
            EmployeeId = id
        });
        return Accepted();
    }
}
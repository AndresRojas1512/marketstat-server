using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimLocation;
using MarketStat.Contracts.Dimensions.DimLocation;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketStat.Gateway.Controllers.Dimensions;

[ApiController]
[Route("api/v1/dimlocations")]
[Authorize]
public class DimLocationController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<IGetDimLocationRequest> _readClient;
    private readonly IRequestClient<IGetAllDimLocationsRequest> _listClient;
    
    private readonly IRequestClient<IGetDistrictsRequest> _districtClient;
    private readonly IRequestClient<IGetOblastsRequest> _oblastClient;
    private readonly IRequestClient<IGetCitiesRequest> _cityClient;

    public DimLocationController(
        IPublishEndpoint publishEndpoint,
        IRequestClient<IGetDimLocationRequest> readClient,
        IRequestClient<IGetAllDimLocationsRequest> listClient,
        IRequestClient<IGetDistrictsRequest> districtClient,
        IRequestClient<IGetOblastsRequest> oblastClient,
        IRequestClient<IGetCitiesRequest> cityClient)
    {
        _publishEndpoint = publishEndpoint;
        _readClient = readClient;
        _listClient = listClient;
        _districtClient = districtClient;
        _oblastClient = oblastClient;
        _cityClient = cityClient;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _listClient.GetResponse<IGetAllDimLocationsResponse>(new { });
        return Ok(response.Message.Locations);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _readClient.GetResponse<IGetDimLocationResponse, IDimLocationNotFoundResponse>(new
        {
            LocationId = id
        });
        if (response.Is(out Response<IGetDimLocationResponse>? success))
            return Ok(success.Message);
        return NotFound();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDimLocationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        await _publishEndpoint.Publish<ISubmitDimLocationCommand>(new
        {
            dto.CityName,
            dto.OblastName,
            dto.DistrictName
        });
        return Accepted();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDimLocationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        await _publishEndpoint.Publish<ISubmitDimLocationUpdateCommand>(new
        {
            LocationId = id,
            dto.CityName,
            dto.OblastName,
            dto.DistrictName
        });
        return Accepted();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _publishEndpoint.Publish<ISubmitDimLocationDeleteCommand>(new
        {
            LocationId = id
        });
        return Accepted();
    }
    
    [HttpGet("lookup/districts")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDistricts()
    {
        var response = await _districtClient.GetResponse<IGetDistrictsResponse>(new { });
        return Ok(response.Message.Districts);
    }

    [HttpGet("lookup/oblasts")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOblasts([FromQuery] string districtName)
    {
        var response = await _oblastClient.GetResponse<IGetOblastsResponse>(new
        {
            DistrictName = districtName
        });
        return Ok(response.Message.Oblasts);
    }

    [HttpGet("lookup/cities")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCities([FromQuery] string oblastName)
    {
        var response = await _cityClient.GetResponse<IGetCitiesResponse>(new
        {
            OblastName = oblastName
        });
        return Ok(response.Message.Cities);
    }
}
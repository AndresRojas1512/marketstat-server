using System.Linq;
using System.Threading.Tasks;
using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Services.Dimensions.DimEmployerService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEmployerServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEmployerService _dimEmployerService;

    private DimEmployer CreateTestEmployerInstance(
        int employerId = 0,
        string employerName = "Test Default Corp",
        string inn = "0123456789",
        string ogrn = "0123456789012",
        string kpp = "012345678",
        string legalAddress = "123 Test St, Testville",
        string website = "http://testdefault.com",
        string contactEmail = "contact@testdefault.com",
        string contactPhone = "555-0100")
    {
        return new DimEmployer(
            employerId,
            employerName,
            inn,
            ogrn,
            kpp,
            new DateOnly(2005, 5, 5),
            legalAddress,
            website,
            contactEmail,
            contactPhone
        );
    }
    
    private (string name, string inn, string ogrn, string kpp, DateOnly regDate, string address, string web, string email, string phone) GetDefaultValidEmployerParams(string name = "Default Corp")
    {
        return (
            name: name,
            inn: "9876543210",
            ogrn: "9876543210987",
            kpp: "987654321",
            regDate: new DateOnly(2010, 10, 10),
            address: "456 Default Ave, AnyCity",
            web: "http://default.org",
            email: "info@default.org",
            phone: "555-0199"
        );
    }

    public DimEmployerServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEmployerService = new DimEmployerService(
            _accessObject.EmployerRepository, 
            NullLogger<DimEmployerService>.Instance
        );
    }
    public void Dispose() => _accessObject.Dispose();

    [Fact]
    public async Task GetAllEmployers_Empty_ReturnsEmpty()
    {
        var all = await _dimEmployerService.GetAllEmployersAsync();

        Assert.Empty(all);
    }

    [Fact]
    public async Task CreateEmployer_PersistsAndGeneratesId()
    {
        var defaultParams = GetDefaultValidEmployerParams("Acme Corp");
        
        var created = await _dimEmployerService.CreateEmployerAsync(
            defaultParams.name, defaultParams.inn, defaultParams.ogrn, defaultParams.kpp,
            defaultParams.regDate, defaultParams.address, defaultParams.web,
            defaultParams.email, defaultParams.phone
        );

        Assert.True(created.EmployerId > 0, "EmployerId should be generated and > 0");
        Assert.Equal(defaultParams.name, created.EmployerName);
        Assert.Equal(defaultParams.inn, created.Inn);
        Assert.Equal(defaultParams.ogrn, created.Ogrn);
        Assert.Equal(defaultParams.kpp, created.Kpp);
        Assert.Equal(defaultParams.regDate, created.RegistrationDate);
        Assert.Equal(defaultParams.address, created.LegalAddress);
        Assert.Equal(defaultParams.web, created.Website);
        Assert.Equal(defaultParams.email, created.ContactEmail);
        Assert.Equal(defaultParams.phone, created.ContactPhone);

        var fetched = await _dimEmployerService.GetEmployerByIdAsync(created.EmployerId);
        Assert.NotNull(fetched);
        Assert.Equal(created.EmployerId, fetched.EmployerId);
        Assert.Equal(defaultParams.name, fetched.EmployerName);
        Assert.Equal(defaultParams.inn, fetched.Inn);
    }

    [Fact]
    public async Task GetEmployerById_Nonexistent_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployerService.GetEmployerByIdAsync(9999)
        );
    }

    [Fact]
    public async Task GetAllEmployers_Seeded_ReturnsSeeded()
    {
        var employer1Params = GetDefaultValidEmployerParams("Foo Inc.");
        var employer2Params = GetDefaultValidEmployerParams("Bar LLC");

        var employersToSeed = new List<DimEmployer>
        {
            new DimEmployer(0, employer1Params.name, employer1Params.inn, employer1Params.ogrn, employer1Params.kpp, employer1Params.regDate, employer1Params.address, employer1Params.web, employer1Params.email, employer1Params.phone),
            new DimEmployer(0, employer2Params.name, employer2Params.inn, employer2Params.ogrn, employer2Params.kpp, employer2Params.regDate, employer2Params.address, employer2Params.web, employer2Params.email, employer2Params.phone)
        };
        
        var created1 = await _dimEmployerService.CreateEmployerAsync(employer1Params.name, employer1Params.inn, employer1Params.ogrn, employer1Params.kpp, employer1Params.regDate, employer1Params.address, employer1Params.web, employer1Params.email, employer1Params.phone);
        var created2 = await _dimEmployerService.CreateEmployerAsync(employer2Params.name, employer2Params.inn, employer2Params.ogrn, employer2Params.kpp, employer2Params.regDate, employer2Params.address, employer2Params.web, employer2Params.email, employer2Params.phone);


        var all = (await _dimEmployerService.GetAllEmployersAsync()).ToList();

        Assert.Equal(2, all.Count);
        Assert.Contains(all, e => e.EmployerId == created1.EmployerId && e.EmployerName == employer1Params.name && e.Inn == employer1Params.inn);
        Assert.Contains(all, e => e.EmployerId == created2.EmployerId && e.EmployerName == employer2Params.name && e.Inn == employer2Params.inn);
    }

    [Fact]
    public async Task UpdateEmployer_PersistsChanges()
    {
        var initialParams = GetDefaultValidEmployerParams("Old Name Corp");
        var created = await _dimEmployerService.CreateEmployerAsync(
            initialParams.name, initialParams.inn, initialParams.ogrn, initialParams.kpp,
            initialParams.regDate, initialParams.address, initialParams.web,
            initialParams.email, initialParams.phone
        );

        var updateParams = GetDefaultValidEmployerParams("New Name Corp Ltd.");

        var updated = await _dimEmployerService.UpdateEmployerAsync(
            created.EmployerId,
            updateParams.name, updateParams.inn, updateParams.ogrn, updateParams.kpp,
            updateParams.regDate, updateParams.address, updateParams.web,
            updateParams.email, updateParams.phone
        );

        Assert.NotNull(updated);
        Assert.Equal(created.EmployerId, updated.EmployerId);
        Assert.Equal(updateParams.name, updated.EmployerName);
        Assert.Equal(updateParams.inn, updated.Inn);
        Assert.Equal(updateParams.ogrn, updated.Ogrn);

        var fetched = await _dimEmployerService.GetEmployerByIdAsync(created.EmployerId);
        Assert.NotNull(fetched);
        Assert.Equal(updateParams.name, fetched.EmployerName);
        Assert.Equal(updateParams.inn, fetched.Inn);
    }

    [Fact]
    public async Task UpdateEmployer_InvalidId_ThrowsArgumentException()
    {
        var defaultParams = GetDefaultValidEmployerParams();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployerService.UpdateEmployerAsync(
                0,
                defaultParams.name, defaultParams.inn, defaultParams.ogrn, defaultParams.kpp,
                defaultParams.regDate, defaultParams.address, defaultParams.web,
                defaultParams.email, defaultParams.phone
            )
        );
    }

    [Fact]
    public async Task UpdateEmployer_NotFound_ThrowsNotFoundException()
    {
        var defaultParams = GetDefaultValidEmployerParams();
        
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployerService.UpdateEmployerAsync(
                9999,
                defaultParams.name, defaultParams.inn, defaultParams.ogrn, defaultParams.kpp,
                defaultParams.regDate, defaultParams.address, defaultParams.web,
                defaultParams.email, defaultParams.phone
            )
        );
    }

    [Fact]
    public async Task DeleteEmployer_RemovesIt()
    {
        var defaultParams = GetDefaultValidEmployerParams("ToDelete Corp");
        var created = await _dimEmployerService.CreateEmployerAsync(
            defaultParams.name, defaultParams.inn, defaultParams.ogrn, defaultParams.kpp,
            defaultParams.regDate, defaultParams.address, defaultParams.web,
            defaultParams.email, defaultParams.phone
        );
        Assert.True(created.EmployerId > 0);
        
        await _dimEmployerService.DeleteEmployerAsync(created.EmployerId);
    
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _dimEmployerService.GetEmployerByIdAsync(created.EmployerId)
        );
    }

    [Fact]
    public async Task DeleteEmployer_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployerService.DeleteEmployerAsync(9999)
        );
    }
}
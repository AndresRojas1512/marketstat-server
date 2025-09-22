using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimEmployerServiceUnitTests
    {
        private readonly Mock<IDimEmployerRepository> _dimEmployerRepositoryMock;
        private readonly Mock<ILogger<DimEmployerService>> _loggerMock;
        private readonly DimEmployerService _dimEmployerService;

        private DimEmployer CreateValidTestEmployer(
            int id = 0,
            string name = "Test Employer",
            string inn = "1234567890",
            string ogrn = "1234567890123",
            string kpp = "123456789",
            string legalAddress = "123 Test St",
            string website = "http://test.com",
            string email = "test@test.com",
            string phone = "555-1234")
        {
            return new DimEmployer(
                id, name, inn, ogrn, kpp,
                DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)),
                legalAddress, website, email, phone
            );
        }
        
        private (string name, string inn, string ogrn, string kpp, DateOnly regDate, string address, string web, string email, string phone) GetDefaultEmployerParams(string name = "Acme Corp")
        {
            return (
                name: name,
                inn: "123456789012",
                ogrn: "1234567890123",
                kpp: "123456789",
                regDate: new DateOnly(2000, 1, 1),
                address: "123 Main St, Anytown",
                web: "http://acme.corp.com",
                email: "contact@acme.corp.com",
                phone: "555-0100"
            );
        }


        public DimEmployerServiceUnitTests()
        {
            _dimEmployerRepositoryMock = new Mock<IDimEmployerRepository>();
            _loggerMock = new Mock<ILogger<DimEmployerService>>();
            _dimEmployerService = new DimEmployerService(
                _dimEmployerRepositoryMock.Object, 
                _loggerMock.Object
            );
        }
    
        [Fact]
        public async Task CreateEmployerAsync_ValidParameters_AssignsIdAndReturns()
        {
            var defaultParams = GetDefaultEmployerParams("Acme Corp Inc.");
            var expectedEmployerId = 42;

            _dimEmployerRepositoryMock.Setup(r => r.AddEmployerAsync(It.IsAny<DimEmployer>()))
                 .Callback<DimEmployer>(e => e.EmployerId = expectedEmployerId)
                 .Returns(Task.CompletedTask);

            var result = await _dimEmployerService.CreateEmployerAsync(
                defaultParams.name, defaultParams.inn, defaultParams.ogrn, defaultParams.kpp,
                defaultParams.regDate, defaultParams.address, defaultParams.web,
                defaultParams.email, defaultParams.phone
            );

            Assert.NotNull(result);
            Assert.Equal(expectedEmployerId, result.EmployerId);
            Assert.Equal(defaultParams.name, result.EmployerName);
            Assert.Equal(defaultParams.inn, result.Inn);
            Assert.Equal(defaultParams.ogrn, result.Ogrn);
            Assert.Equal(defaultParams.kpp, result.Kpp);
            Assert.Equal(defaultParams.regDate, result.RegistrationDate);
            Assert.Equal(defaultParams.address, result.LegalAddress);
            Assert.Equal(defaultParams.web, result.Website);
            Assert.Equal(defaultParams.email, result.ContactEmail);
            Assert.Equal(defaultParams.phone, result.ContactPhone);

            _dimEmployerRepositoryMock.Verify(r => r.AddEmployerAsync(
                It.Is<DimEmployer>(e =>
                    e.EmployerName == defaultParams.name &&
                    e.Inn == defaultParams.inn &&
                    e.Ogrn == defaultParams.ogrn
            )), Times.Once);
        }

        [Fact]
        public async Task CreateEmployerAsync_RepositoryThrowsConflict_ThrowsConflictException()
        {
            var defaultParams = GetDefaultEmployerParams("Conflict Corp");
            _dimEmployerRepositoryMock.Setup(r => r.AddEmployerAsync(It.IsAny<DimEmployer>()))
                 .ThrowsAsync(new ConflictException("Duplicate employer"));

            await Assert.ThrowsAsync<ConflictException>(() =>
                _dimEmployerService.CreateEmployerAsync(
                    defaultParams.name, defaultParams.inn, defaultParams.ogrn, defaultParams.kpp,
                    defaultParams.regDate, defaultParams.address, defaultParams.web,
                    defaultParams.email, defaultParams.phone
                )
            );
        }

        [Fact]
        public async Task CreateEmployerAsync_InvalidName_ThrowsArgumentException()
        {
            var defaultParams = GetDefaultEmployerParams("");

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _dimEmployerService.CreateEmployerAsync(
                    defaultParams.name, defaultParams.inn, defaultParams.ogrn, defaultParams.kpp,
                    defaultParams.regDate, defaultParams.address, defaultParams.web,
                    defaultParams.email, defaultParams.phone
                )
            );
        }

        [Fact]
        public async Task GetEmployerByIdAsync_Found_ReturnsEntity()
        {
            var expectedEmployer = CreateValidTestEmployer(7, "Found Corp");
            _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(7))
                 .ReturnsAsync(expectedEmployer);

            var actual = await _dimEmployerService.GetEmployerByIdAsync(7);

            Assert.Same(expectedEmployer, actual);
        }

        [Fact]
        public async Task GetEmployerByIdAsync_NotFound_ThrowsNotFoundException()
        {
            _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(9))
                 .ThrowsAsync(new NotFoundException("nope"));
            
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _dimEmployerService.GetEmployerByIdAsync(9)
            );
        }

        [Fact]
        public async Task GetAllEmployersAsync_ReturnsAll()
        {
            var list = new List<DimEmployer>
            {
                CreateValidTestEmployer(1, "Alpha Inc."),
                CreateValidTestEmployer(2, "Beta LLC")
            };
            _dimEmployerRepositoryMock.Setup(r => r.GetAllEmployersAsync())
                 .ReturnsAsync(list);

            var result = await _dimEmployerService.GetAllEmployersAsync();

            Assert.Equal(list.Count, result.Count());
            Assert.Equal(list, result);
        }

        [Fact]
        public async Task UpdateEmployerAsync_ValidParameters_UpdatesAndReturns()
        {
            var existing = CreateValidTestEmployer(3, "Old Name");
            var updateParams = GetDefaultEmployerParams("New Name Inc.");

            _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(3)).ReturnsAsync(existing);
            _dimEmployerRepositoryMock.Setup(r => r.UpdateEmployerAsync(It.IsAny<DimEmployer>())).Returns(Task.CompletedTask);

            var updated = await _dimEmployerService.UpdateEmployerAsync(
                3, updateParams.name, updateParams.inn, updateParams.ogrn, updateParams.kpp,
                updateParams.regDate, updateParams.address, updateParams.web,
                updateParams.email, updateParams.phone
            );

            Assert.NotNull(updated);
            Assert.Equal(updateParams.name, updated.EmployerName);
            Assert.Equal(updateParams.inn, updated.Inn);
            Assert.Equal(updateParams.ogrn, updated.Ogrn);


            _dimEmployerRepositoryMock.Verify(r => r.UpdateEmployerAsync(
                It.Is<DimEmployer>(e =>
                    e.EmployerId   == 3 &&
                    e.EmployerName == updateParams.name &&
                    e.Inn == updateParams.inn
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateEmployerAsync_InvalidId_ThrowsArgumentException()
        {
            var defaultParams = GetDefaultEmployerParams();
            
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _dimEmployerService.UpdateEmployerAsync(
                    0, defaultParams.name, defaultParams.inn, defaultParams.ogrn, defaultParams.kpp,
                    defaultParams.regDate, defaultParams.address, defaultParams.web,
                    defaultParams.email, defaultParams.phone
                )
            );
        }

        [Fact]
        public async Task UpdateEmployerAsync_NotFound_ThrowsNotFoundException()
        {
            var updateParams = GetDefaultEmployerParams();
            _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(5))
                 .ThrowsAsync(new NotFoundException("missing"));

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _dimEmployerService.UpdateEmployerAsync(
                    5, updateParams.name, updateParams.inn, updateParams.ogrn, updateParams.kpp,
                    updateParams.regDate, updateParams.address, updateParams.web,
                    updateParams.email, updateParams.phone
                )
            );
        }

        [Fact]
        public async Task UpdateEmployerAsync_Conflict_ThrowsConflictException()
        {
            var existing = CreateValidTestEmployer(6, "Original Corp");
            var updateParams = GetDefaultEmployerParams("Conflicting Name Corp");
            _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(6)).ReturnsAsync(existing);
            _dimEmployerRepositoryMock.Setup(r => r.UpdateEmployerAsync(It.IsAny<DimEmployer>()))
                 .ThrowsAsync(new ConflictException("dupe"));

            await Assert.ThrowsAsync<ConflictException>(() =>
                _dimEmployerService.UpdateEmployerAsync(
                    6, updateParams.name, updateParams.inn, updateParams.ogrn, updateParams.kpp,
                    updateParams.regDate, updateParams.address, updateParams.web,
                    updateParams.email, updateParams.phone
                )
            );
        }

        [Fact]
        public async Task DeleteEmployerAsync_ValidId_Completes()
        {
            _dimEmployerRepositoryMock.Setup(r => r.DeleteEmployerAsync(8)).Returns(Task.CompletedTask);

            await _dimEmployerService.DeleteEmployerAsync(8);

            _dimEmployerRepositoryMock.Verify(r => r.DeleteEmployerAsync(8), Times.Once);
        }

        [Fact]
        public async Task DeleteEmployerAsync_NotFound_ThrowsNotFoundException()
        {
            _dimEmployerRepositoryMock.Setup(r => r.DeleteEmployerAsync(9))
                 .ThrowsAsync(new NotFoundException("gone"));

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _dimEmployerService.DeleteEmployerAsync(9)
            );
        }
    }
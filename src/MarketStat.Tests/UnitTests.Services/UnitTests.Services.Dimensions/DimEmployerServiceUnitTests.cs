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

        // Helper to create a valid DimEmployer with dummy data for new fields
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
                DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)), // RegistrationDate
                legalAddress, website, email, phone
            );
        }
        
        // Helper to provide default valid parameters for service methods
        private (string name, string inn, string ogrn, string kpp, DateOnly regDate, string address, string web, string email, string phone) GetDefaultEmployerParams(string name = "Acme Corp")
        {
            return (
                name: name,
                inn: "123456789012", // Max 12
                ogrn: "1234567890123", // 13
                kpp: "123456789", // 9
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
            // DimEmployerService constructor now only takes repository and logger
            _dimEmployerService = new DimEmployerService(
                _dimEmployerRepositoryMock.Object, 
                _loggerMock.Object
                // IMapper is no longer injected into this service based on your provided service code
            );
        }
    
        [Fact]
        public async Task CreateEmployerAsync_ValidParameters_AssignsIdAndReturns()
        {
            // Arrange
            var defaultParams = GetDefaultEmployerParams("Acme Corp Inc.");
            var expectedEmployerId = 42;

            _dimEmployerRepositoryMock.Setup(r => r.AddEmployerAsync(It.IsAny<DimEmployer>()))
                 .Callback<DimEmployer>(e => e.EmployerId = expectedEmployerId) // Simulate ID generation
                 .Returns(Task.CompletedTask);

            // Act
            var result = await _dimEmployerService.CreateEmployerAsync(
                defaultParams.name, defaultParams.inn, defaultParams.ogrn, defaultParams.kpp,
                defaultParams.regDate, defaultParams.address, defaultParams.web,
                defaultParams.email, defaultParams.phone
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEmployerId, result.EmployerId);
            Assert.Equal(defaultParams.name, result.EmployerName);
            Assert.Equal(defaultParams.inn, result.Inn);
            // ... Assert other new properties ...
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
                    e.Ogrn == defaultParams.ogrn // etc. for other fields
            )), Times.Once);
        }

        [Fact]
        public async Task CreateEmployerAsync_RepositoryThrowsConflict_ThrowsConflictException()
        {
            // Arrange
            var defaultParams = GetDefaultEmployerParams("Conflict Corp");
            _dimEmployerRepositoryMock.Setup(r => r.AddEmployerAsync(It.IsAny<DimEmployer>()))
                 .ThrowsAsync(new ConflictException("Duplicate employer"));

            // Act & Assert
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
            // Arrange: DimEmployerValidator is called first by the service.
            // This test now relies on DimEmployerValidator to throw.
            var defaultParams = GetDefaultEmployerParams(""); // Invalid empty name

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => // Or whatever DimEmployerValidator throws
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
            // Arrange
            var expectedEmployer = CreateValidTestEmployer(7, "Found Corp");
            _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(7))
                 .ReturnsAsync(expectedEmployer);

            // Act
            var actual = await _dimEmployerService.GetEmployerByIdAsync(7);

            // Assert
            Assert.Same(expectedEmployer, actual);
        }

        [Fact]
        public async Task GetEmployerByIdAsync_NotFound_ThrowsNotFoundException()
        {
            // Arrange
            _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(9))
                 .ThrowsAsync(new NotFoundException("nope")); // Repository throws this

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _dimEmployerService.GetEmployerByIdAsync(9)
            );
        }

        [Fact]
        public async Task GetAllEmployersAsync_ReturnsAll()
        {
            // Arrange
            var list = new List<DimEmployer>
            {
                CreateValidTestEmployer(1, "Alpha Inc."),
                CreateValidTestEmployer(2, "Beta LLC")
            };
            _dimEmployerRepositoryMock.Setup(r => r.GetAllEmployersAsync())
                 .ReturnsAsync(list);

            // Act
            var result = await _dimEmployerService.GetAllEmployersAsync();

            // Assert
            Assert.Equal(list.Count, result.Count());
            Assert.Equal(list, result);
        }

        [Fact]
        public async Task UpdateEmployerAsync_ValidParameters_UpdatesAndReturns()
        {
            // Arrange
            var existing = CreateValidTestEmployer(3, "Old Name");
            var updateParams = GetDefaultEmployerParams("New Name Inc.");

            _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(3)).ReturnsAsync(existing);
            _dimEmployerRepositoryMock.Setup(r => r.UpdateEmployerAsync(It.IsAny<DimEmployer>())).Returns(Task.CompletedTask);

            // Act
            var updated = await _dimEmployerService.UpdateEmployerAsync(
                3, updateParams.name, updateParams.inn, updateParams.ogrn, updateParams.kpp,
                updateParams.regDate, updateParams.address, updateParams.web,
                updateParams.email, updateParams.phone
            );

            // Assert
            Assert.NotNull(updated);
            Assert.Equal(updateParams.name, updated.EmployerName);
            Assert.Equal(updateParams.inn, updated.Inn);
            // ... Assert other updated properties ...
            Assert.Equal(updateParams.ogrn, updated.Ogrn);


            _dimEmployerRepositoryMock.Verify(r => r.UpdateEmployerAsync(
                It.Is<DimEmployer>(e =>
                    e.EmployerId   == 3 &&
                    e.EmployerName == updateParams.name &&
                    e.Inn == updateParams.inn // etc.
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateEmployerAsync_InvalidId_ThrowsArgumentException()
        {
            // Arrange - DimEmployerValidator is called first
            var defaultParams = GetDefaultEmployerParams();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => // Or whatever DimEmployerValidator throws for ID
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
            // Arrange
            var updateParams = GetDefaultEmployerParams();
            _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(5))
                 .ThrowsAsync(new NotFoundException("missing")); // GetByIdAsync throws

            // Act & Assert
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
            // Arrange
            var existing = CreateValidTestEmployer(6, "Original Corp");
            var updateParams = GetDefaultEmployerParams("Conflicting Name Corp");
            _dimEmployerRepositoryMock.Setup(r => r.GetEmployerByIdAsync(6)).ReturnsAsync(existing);
            _dimEmployerRepositoryMock.Setup(r => r.UpdateEmployerAsync(It.IsAny<DimEmployer>()))
                 .ThrowsAsync(new ConflictException("dupe"));

            // Act & Assert
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
            // Arrange
            _dimEmployerRepositoryMock.Setup(r => r.DeleteEmployerAsync(8)).Returns(Task.CompletedTask);

            // Act
            await _dimEmployerService.DeleteEmployerAsync(8);

            // Assert
            _dimEmployerRepositoryMock.Verify(r => r.DeleteEmployerAsync(8), Times.Once);
        }

        [Fact]
        public async Task DeleteEmployerAsync_NotFound_ThrowsNotFoundException()
        {
            // Arrange
            _dimEmployerRepositoryMock.Setup(r => r.DeleteEmployerAsync(9))
                 .ThrowsAsync(new NotFoundException("gone"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _dimEmployerService.DeleteEmployerAsync(9)
            );
        }
    }
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerIndustryFieldService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimEmployerIndustryFieldServiceUnitTests
{
    private readonly Mock<IDimEmployerIndustryFieldRepository> _dimEmployerIndustryFieldRepositoryMock;
    private readonly Mock<ILogger<DimEmployerIndustryFieldService>> _loggerMock;
    private readonly DimEmployerIndustryFieldService _dimEmployerIndustryFieldService;

    public DimEmployerIndustryFieldServiceUnitTests()
    {
        _dimEmployerIndustryFieldRepositoryMock = new Mock<IDimEmployerIndustryFieldRepository>();
        _loggerMock = new Mock<ILogger<DimEmployerIndustryFieldService>>();
        _dimEmployerIndustryFieldService =
            new DimEmployerIndustryFieldService(_dimEmployerIndustryFieldRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
        public async Task CreateEmployerIndustryFieldAsync_ValidParameters_CreatesLink()
        {
            const int empId = 1, indId = 2;
            _dimEmployerIndustryFieldRepositoryMock
                .Setup(r => r.AddEmployerIndustryFieldAsync(It.IsAny<DimEmployerIndustryField>()))
                .Returns(Task.CompletedTask);

            var link = await _dimEmployerIndustryFieldService.CreateEmployerIndustryFieldAsync(empId, indId);

            Assert.Equal(empId, link.EmployerId);
            Assert.Equal(indId, link.IndustryFieldId);
            _dimEmployerIndustryFieldRepositoryMock.Verify(r =>
                r.AddEmployerIndustryFieldAsync(
                    It.Is<DimEmployerIndustryField>(l =>
                        l.EmployerId      == empId &&
                        l.IndustryFieldId == indId
                    )), Times.Once);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public async Task CreateEmployerIndustryFieldAsync_InvalidParameters_ThrowsArgumentException(int empId, int indId)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _dimEmployerIndustryFieldService.CreateEmployerIndustryFieldAsync(empId, indId));
        }

        [Fact]
        public async Task CreateEmployerIndustryFieldAsync_Conflict_ThrowsConflictException()
        {
            const int empId = 3, indId = 4;
            var message = $"Link ({empId}, {indId}) already exists.";
            _dimEmployerIndustryFieldRepositoryMock
                .Setup(r => r.AddEmployerIndustryFieldAsync(It.IsAny<DimEmployerIndustryField>()))
                .ThrowsAsync(new ConflictException(message));

            var ex = await Assert.ThrowsAsync<ConflictException>(() =>
                _dimEmployerIndustryFieldService.CreateEmployerIndustryFieldAsync(empId, indId));

            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public async Task CreateEmployerIndustryFieldAsync_FkNotFound_ThrowsNotFoundException()
        {
            const int empId = 5, indId = 6;
            var message = $"Referenced employer {empId} or industry field {indId} not found.";
            _dimEmployerIndustryFieldRepositoryMock
                .Setup(r => r.AddEmployerIndustryFieldAsync(It.IsAny<DimEmployerIndustryField>()))
                .ThrowsAsync(new NotFoundException(message));

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _dimEmployerIndustryFieldService.CreateEmployerIndustryFieldAsync(empId, indId));

            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public async Task GetEmployerIndustryFieldAsync_Existing_ReturnsLink()
        {
            var expected = new DimEmployerIndustryField(7, 8);
            _dimEmployerIndustryFieldRepositoryMock
                .Setup(r => r.GetEmployerIndustryFieldAsync(7, 8))
                .ReturnsAsync(expected);

            var actual = await _dimEmployerIndustryFieldService.GetEmployerIndustryFieldAsync(7, 8);

            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task GetEmployerIndustryFieldAsync_NotFound_ThrowsNotFoundException()
        {
            const int empId = 1, indId = 2;
            var message = $"EmployeeEducation ({empId}, {indId}) not found.";
            _dimEmployerIndustryFieldRepositoryMock
                .Setup(r => r.GetEmployerIndustryFieldAsync(empId, indId))
                .ThrowsAsync(new NotFoundException(message));

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _dimEmployerIndustryFieldService.GetEmployerIndustryFieldAsync(empId, indId));

            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public async Task GetIndustryFieldsByEmployeeIdAsync_ReturnsLinks()
        {
            const int empId = 9;
            var list = new[]
            {
                new DimEmployerIndustryField(empId, 10),
                new DimEmployerIndustryField(empId, 11)
            };
            _dimEmployerIndustryFieldRepositoryMock
                .Setup(r => r.GetIndustryFieldsByEmployerIdAsync(empId))
                .ReturnsAsync(list);

            var result = (await _dimEmployerIndustryFieldService.GetIndustryFieldsByEmployerIdAsync(empId)).ToList();

            Assert.Equal(list, result);
        }

        [Fact]
        public async Task GetEmployersByIndustryFieldIdAsync_ReturnsLinks()
        {
            const int indId = 12;
            var list = new[]
            {
                new DimEmployerIndustryField(20, indId),
                new DimEmployerIndustryField(21, indId)
            };
            _dimEmployerIndustryFieldRepositoryMock
                .Setup(r => r.GetEmployersByIndustryFieldIdAsync(indId))
                .ReturnsAsync(list);

            var result = (await _dimEmployerIndustryFieldService.GetEmployersByIndustryFieldIdAsync(indId)).ToList();

            Assert.Equal(list, result);
        }

        [Fact]
        public async Task GetAllEmployerIndustryFieldsAsync_ReturnsAllLinks()
        {
            var list = new[]
            {
                new DimEmployerIndustryField(1, 2),
                new DimEmployerIndustryField(3, 4),
                new DimEmployerIndustryField(5, 6)
            };
            _dimEmployerIndustryFieldRepositoryMock
                .Setup(r => r.GetAllEmployerIndustryFieldsAsync())
                .ReturnsAsync(list);

            var result = (await _dimEmployerIndustryFieldService.GetAllEmployerIndustryFieldsAsync()).ToList();

            Assert.Equal(list, result);
        }

        [Fact]
        public async Task DeleteEmployerIndustryFieldAsync_ValidParameters_CallsRepository()
        {
            const int empId = 13, indId = 14;
            _dimEmployerIndustryFieldRepositoryMock
                .Setup(r => r.DeleteEmployerIndustryFieldAsync(empId, indId))
                .Returns(Task.CompletedTask);

            await _dimEmployerIndustryFieldService.DeleteEmployerIndustryFieldAsync(empId, indId);

            _dimEmployerIndustryFieldRepositoryMock.Verify(r =>
                r.DeleteEmployerIndustryFieldAsync(empId, indId), Times.Once);
        }

    [Fact]
    public async Task DeleteEmployerIndustryFieldAsync_NotFound_ThrowsNotFoundException()
    {
        const int empId = 15, indId = 16;
        var message = $"Cannot delete: EmployeeEducation ({empId}, {indId}) not found.";
        _dimEmployerIndustryFieldRepositoryMock
            .Setup(r => r.DeleteEmployerIndustryFieldAsync(empId, indId))
            .ThrowsAsync(new NotFoundException(message));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployerIndustryFieldService.DeleteEmployerIndustryFieldAsync(empId, indId));

        Assert.Equal(message, ex.Message);
    }
}
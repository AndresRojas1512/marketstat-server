using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Services.Dimensions.DimDateService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimDateServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimDateService _dimDateService;

    public DimDateServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimDateService = new DimDateService(_accessObject.DimDateRepository, NullLogger<DimDateService>.Instance);
    }
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
        public async Task GetAllDates_Empty_ReturnsEmpty()
        {
            var all = await _dimDateService.GetAllDatesAsync();
            Assert.Empty(all);
        }

        [Fact]
        public async Task GetAllDates_Seeded_ReturnsSeeded()
        {
            var seed = new[]
            {
                new DimDate(1, new DateOnly(2020,  1,  1), 2020, 1, 1),
                new DimDate(2, new DateOnly(2021,  6, 15), 2021, 2, 6)
            };
            await _accessObject.SeedDateAsync(seed);

            var all = (await _dimDateService.GetAllDatesAsync()).ToList();
            Assert.Equal(2, all.Count);
            Assert.Contains(all, d => d.FullDate == seed[0].FullDate && d.Year == seed[0].Year);
            Assert.Contains(all, d => d.FullDate == seed[1].FullDate && d.Year == seed[1].Year);
        }

        [Fact]
        public async Task CreateDate_PersistsAndRetrievable()
        {
            var fullDate = new DateOnly(2022,  3, 14);
            var created  = await _dimDateService.CreateDateAsync(fullDate);

            Assert.True(created.DateId > 0);
            Assert.Equal(fullDate,       created.FullDate);
            Assert.Equal(fullDate.Year,  created.Year);
            Assert.Equal(1,              created.Quarter);
            Assert.Equal(fullDate.Month, created.Month);

            var fetched = await _dimDateService.GetDateByIdAsync(created.DateId);
            Assert.Equal(created.FullDate, fetched.FullDate);
            Assert.Equal(created.Year,     fetched.Year);
        }

        [Fact]
        public async Task GetDateById_NonExisting_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _dimDateService.GetDateByIdAsync(999)
            );
        }

        [Fact]
        public async Task UpdateDate_PersistsChanges()
        {
            var initial = await _dimDateService.CreateDateAsync(new DateOnly(2022,  8,  5));
            var updated = await _dimDateService.UpdateDateAsync(
                initial.DateId,
                new DateOnly(2023, 11, 30)
            );

            Assert.Equal(initial.DateId,      updated.DateId);
            Assert.Equal(new DateOnly(2023, 11, 30), updated.FullDate);
            Assert.Equal(2023,                updated.Year);
            Assert.Equal(4,                   updated.Quarter);
            Assert.Equal(11,                  updated.Month);

            var fetched = await _dimDateService.GetDateByIdAsync(initial.DateId);
            Assert.Equal(updated.FullDate, fetched.FullDate);
        }

        [Fact]
        public async Task UpdateDate_NonExisting_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _dimDateService.UpdateDateAsync(1234, new DateOnly(2025, 1, 1))
            );
        }

        [Fact]
        public async Task DeleteDate_RemovesIt()
        {
            var created = await _dimDateService.CreateDateAsync(new DateOnly(2024, 2, 28));
            await _dimDateService.DeleteDateAsync(created.DateId);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _dimDateService.GetDateByIdAsync(created.DateId)
            );
        }

    [Fact]
    public async Task DeleteDate_NonExisting_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimDateService.DeleteDateAsync(5678)
        );
    }
}
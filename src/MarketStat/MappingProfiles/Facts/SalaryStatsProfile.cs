using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

namespace MarketStat.MappingProfiles.Facts;

public class SalaryStatsProfile : Profile
{
    public SalaryStatsProfile()
    {
        CreateMap<SalaryStats, SalaryStatsDto>();
    }
}
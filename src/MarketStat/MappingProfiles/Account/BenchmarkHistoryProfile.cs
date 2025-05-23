using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;

namespace MarketStat.MappingProfiles.Account;

public class BenchmarkHistoryProfile : Profile
{
    public BenchmarkHistoryProfile()
    {
        CreateMap<BenchmarkHistory, BenchmarkHistoryDto>()
            .ForMember(
                dest => dest.Username,
                opt => opt.MapFrom(src => src.User != null ? src.User.Username : null) // Get it from the User navigation property
            );
    }
}
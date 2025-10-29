using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;

namespace MarketStat.MappingProfiles.Account;

public class BenchmarkHistoryProfile : Profile
{
    public BenchmarkHistoryProfile()
    {
        CreateMap<BenchmarkHistory, BenchmarkHistoryDto>()
            .ForMember(dest => dest.BenchmarkHistoryId, opt => opt.MapFrom(src => src.BenchmarkHistoryId))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.BenchmarkName, opt => opt.MapFrom(src => src.BenchmarkName))
            .ForMember(dest => dest.SavedAt, opt => opt.MapFrom(src => src.SavedAt))

            .ForMember(dest => dest.FilterDateStart, opt => opt.MapFrom(src => src.FilterDateStart))
            .ForMember(dest => dest.FilterDateEnd, opt => opt.MapFrom(src => src.FilterDateEnd))
            .ForMember(dest => dest.FilterTargetPercentile, opt => opt.MapFrom(src => src.FilterTargetPercentile))
            .ForMember(dest => dest.FilterGranularity, opt => opt.MapFrom(src => src.FilterGranularity))
            .ForMember(dest => dest.FilterPeriods, opt => opt.MapFrom(src => src.FilterPeriods))
            .ForMember(dest => dest.BenchmarkResultJson, opt => opt.MapFrom(src => src.BenchmarkResultJson))

            .ForMember(
                dest => dest.Username,
                opt => opt.MapFrom(src => (src.User != null) ? src.User.Username : null)
            )

            .ForMember(dest => dest.FilterIndustryFieldId, opt => opt.MapFrom(src => src.FilterIndustryFieldId))
            .ForMember(dest => dest.FilterStandardJobRoleTitle,
                opt => opt.MapFrom(src => src.FilterStandardJobRoleTitle))
            .ForMember(dest => dest.FilterHierarchyLevelName, opt => opt.MapFrom(src => src.FilterHierarchyLevelName))
            .ForMember(dest => dest.FilterDistrictName, opt => opt.MapFrom(src => src.FilterDistrictName))
            .ForMember(dest => dest.FilterOblastName, opt => opt.MapFrom(src => src.FilterOblastName))
            .ForMember(dest => dest.FilterCityName, opt => opt.MapFrom(src => src.FilterCityName));

        CreateMap<SaveBenchmarkRequestDto, BenchmarkHistory>()
            .ForMember(dest => dest.BenchmarkHistoryId, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.SavedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
        
        CreateMap<User, UserDto>();
    }
}
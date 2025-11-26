using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Responses;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;

namespace MarketStat.MappingProfiles.Facts;

public class FactSalaryProfile : Profile
{
    public FactSalaryProfile()
    {
        CreateMap<FactSalary, FactSalaryDto>();

        CreateMap<CreateFactSalaryDto, FactSalary>()
            .ForMember(dest => dest.SalaryFactId, opt => opt.Ignore());
        
        CreateMap<UpdateFactSalaryDto, FactSalary>()
            .ForMember(dest => dest.SalaryFactId, opt => opt.Ignore());

        CreateMap<SalaryFilterDto, AnalysisFilterRequest>();

        CreateMap<SalarySummaryRequestDto, SalarySummaryRequest>();
        CreateMap<SalaryTimeSeriesRequestDto, TimeSeriesRequest>();
        CreateMap<PublicRolesRequestDto, PublicRolesRequest>();

        CreateMap<SalarySummary, SalarySummaryDto>();
        CreateMap<SalaryDistributionBucket, SalaryDistributionBucketDto>();
        CreateMap<SalaryTimeSeriesPoint, SalaryTimeSeriesPointDto>();

        CreateMap<PublicRoleByLocationIndustry, PublicRoleByLocationIndustryDto>();
    }
}
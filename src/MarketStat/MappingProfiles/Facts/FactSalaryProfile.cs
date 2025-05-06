using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

namespace MarketStat.MappingProfiles.Facts;

public class FactSalaryProfile : Profile
{
    public FactSalaryProfile()
    {
        CreateMap<FactSalary, FactSalaryDto>();
        
        CreateMap<CreateFactSalaryDto, FactSalary>();
        
        CreateMap<UpdateFactSalaryDto, FactSalary>()
            .ForMember(dest => dest.SalaryFactId, opt => opt.Ignore());
    }
}